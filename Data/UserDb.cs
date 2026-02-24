using System.Data;
using Microsoft.Data.SqlClient;
using HelpdeskSystem.Models;
using System.Text;
using System.Security.Cryptography;

namespace HelpdeskSystem.Data
{
    // Data access for Users table using parameterized ADO.NET queries
    public class UserDb
    {
        private readonly Db _db;

        public UserDb(Db db)
        {
            _db = db;
        }

        // Retrieve user by email
        public (int Id, string FullName, string PasswordHash, bool IsActive)? GetByEmail(string email)
        {
            using var connection = _db.CreateConnection();
            const string sql = "SELECT Id, FullName, PasswordHash, IsActive FROM Users WHERE Email = @Email";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email ?? string.Empty });

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var id = reader.GetInt32(0);
                var fullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                var passwordHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var isActive = !reader.IsDBNull(3) && reader.GetBoolean(3);
                return (id, fullName, passwordHash, isActive);
            }
            return null;
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var connection = _db.CreateConnection();
            const string sql = "SELECT Id, FullName, Email, IsActive, CreatedDate FROM Users ORDER BY FullName";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new User
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = !reader.IsDBNull(3) && reader.GetBoolean(3),
                    CreatedDate = !reader.IsDBNull(4) ? reader.GetDateTime(4) : DateTime.MinValue
                });
            }
            return list;
        }

        public bool EmailExists(string email)
        {
            using var connection = _db.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = email ?? string.Empty });

            connection.Open();
            var count = (int)command.ExecuteScalar();
            return count > 0;
        }

        public void InsertUser(User model)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"INSERT INTO Users (FullName, Email, PasswordHash, IsActive, CreatedDate)
VALUES (@FullName, @Email, @PasswordHash, @IsActive, @CreatedDate)";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = model.FullName ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = model.Email ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 256) { Value = model.PasswordHash ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = model.IsActive });
            command.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = model.CreatedDate });

            connection.Open();
            command.ExecuteNonQuery();
        }

        // Compute SHA256 HEX
        public static string ComputeSha256Hash(string input)
        {
            input ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
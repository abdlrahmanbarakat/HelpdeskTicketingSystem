using System.Data;
using Microsoft.Data.SqlClient;
using HelpdeskSystem.Models;
using System.Text;
using System.Security.Cryptography;

namespace HelpdeskSystem.Data
{
    /// <summary>
    /// Simple data-access class for retrieving and manipulating user information.
    /// Uses ADO.NET directly with parameterized queries and using statements.
    /// </summary>
    public class UserDb
    {
        private readonly Db _db;

        public UserDb(Db db)
        {
            _db = db;
        }

        /// <summary>
        /// Retrieves a user by email address.
        /// Returns a tuple with (Id, FullName, PasswordHash, IsActive) or null if not found.
        ///
        /// Steps explained:
        /// - Create a SqlConnection using the Db helper.
        /// - Create a parameterized SqlCommand to avoid SQL injection.
        /// - Open the connection and execute a SqlDataReader to read results.
        /// - Use using blocks to ensure connections and readers are disposed promptly.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>Tuple (Id, FullName, PasswordHash, IsActive)? or null if no user found.</returns>
        public (int Id, string FullName, string PasswordHash, bool IsActive)? GetByEmail(string email)
        {
            // Create a new SqlConnection from the Db helper. The connection is not opened yet.
            // We wrap it in a using statement so the connection is closed and disposed automatically,
            // even if an exception occurs. This prevents connection leaks.
            using var connection = _db.CreateConnection();

            // Create the SQL query. Use a parameter @Email rather than interpolating the value directly
            // to prevent SQL injection attacks and to let the database optimize the query plan.
            const string sql = "SELECT Id, FullName, PasswordHash, IsActive FROM Users WHERE Email = @Email";

            // Create a SqlCommand associated with the connection and the SQL text.
            using var command = new SqlCommand(sql, connection)
            {
                CommandType = CommandType.Text
            };

            // Add the parameter for @Email in a parameterized way. This ensures the value is treated
            // as data and not executable SQL. Adjust size if you know the column length; here 256 is a safe default.
            var emailParam = new SqlParameter("@Email", SqlDbType.NVarChar, 256)
            {
                Value = email ?? string.Empty
            };
            command.Parameters.Add(emailParam);

            // Open the connection before executing the command.
            connection.Open();

            // Execute the command and obtain a reader. The reader provides forward-only access to the results.
            // We use a using statement so the reader is closed and disposed after we finish reading.
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                // Read values from the reader by column ordinal. Ordinals match the SELECT list order.
                // Use appropriate Get methods to avoid boxing/unboxing and to get proper types.
                var id = reader.GetInt32(0); // Id
                var fullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1); // FullName may be nullable
                var passwordHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2); // PasswordHash
                var isActive = !reader.IsDBNull(3) && reader.GetBoolean(3); // IsActive

                // Return the tuple with the found user data.
                return (id, fullName, passwordHash, isActive);
            }

            // If no rows were returned, return null to indicate user not found.
            return null;
        }

        /// <summary>
        /// Get all users for display in an index/list view.
        /// </summary>
        /// <returns>List of users.</returns>
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
                var u = new User
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Email = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    IsActive = !reader.IsDBNull(3) && reader.GetBoolean(3),
                    CreatedDate = !reader.IsDBNull(4) ? reader.GetDateTime(4) : DateTime.MinValue
                };
                list.Add(u);
            }

            return list;
        }

        /// <summary>
        /// Checks whether an email already exists in the Users table.
        /// </summary>
        /// <param name="email">Email to check.</param>
        /// <returns>True if exists, otherwise false.</returns>
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

        /// <summary>
        /// Inserts a new user into the database. PasswordHash must be provided (SHA256 HEX) in the model.
        /// </summary>
        /// <param name="model">User model with FullName, Email, PasswordHash, IsActive, CreatedDate.</param>
        public void InsertUser(User model)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"
INSERT INTO Users (FullName, Email, PasswordHash, IsActive, CreatedDate)
VALUES (@FullName, @Email, @PasswordHash, @IsActive, @CreatedDate)
";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar, 200) { Value = model.FullName ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256) { Value = model.Email ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 256) { Value = model.PasswordHash ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = model.IsActive });
            command.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = model.CreatedDate });

            connection.Open();
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Helper to compute SHA256 HEX string (uppercase) for password hashing to match DB format.
        /// </summary>
        public static string ComputeSha256Hash(string input)
        {
            input ??= string.Empty;
            var bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(bytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
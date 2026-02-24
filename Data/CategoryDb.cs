using System.Data;
using Microsoft.Data.SqlClient;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Data
{
    public class CategoryDb
    {
        private readonly Db _db;

        public CategoryDb(Db db)
        {
            _db = db;
        }

        public List<Category> GetAllCategories()
        {
            var list = new List<Category>();
            using var connection = _db.CreateConnection();
            const string sql = "SELECT Id, Name, IsActive, CreatedDate FROM Categories ORDER BY Name";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    IsActive = reader.GetBoolean(2),
                    CreatedDate = reader.GetDateTime(3)
                });
            }

            return list;
        }

        public bool NameExists(string name)
        {
            using var connection = _db.CreateConnection();
            const string sql = "SELECT COUNT(1) FROM Categories WHERE Name = @Name";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = name ?? string.Empty });

            connection.Open();
            var result = (int)command.ExecuteScalar()!;
            return result > 0;
        }

        public void InsertCategory(Category model)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"INSERT INTO Categories (Name, IsActive, CreatedDate)
VALUES (@Name, @IsActive, @CreatedDate)";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = model.Name ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = model.IsActive });
            command.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = model.CreatedDate });

            connection.Open();
            command.ExecuteNonQuery();
        }

        public bool ToggleIsActive(int id)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"UPDATE Categories
SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
WHERE Id = @Id";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.AddWithValue("@Id", id);

            connection.Open();
            var rows = command.ExecuteNonQuery();
            return rows > 0;
        }
    }
}
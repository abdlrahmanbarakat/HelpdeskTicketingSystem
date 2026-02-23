using System.Data;
using Microsoft.Data.SqlClient;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Data
{
    /// <summary>
    /// Simple ADO.NET data access for Categories table.
    /// </summary>
    public class CategoryDb
    {
        private readonly Db _db;

        public CategoryDb(Db db)
        {
            _db = db;
        }

        /// <summary>
        /// Returns all categories ordered by name.
        /// SELECT Id, Name, IsActive, CreatedDate FROM Categories ORDER BY Name
        /// </summary>
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

        /// <summary>
        /// Checks if a category name already exists in the database.
        /// Returns true if the name exists, false otherwise.
        /// SELECT COUNT(1) FROM Categories WHERE Name = @Name
        /// </summary>
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

        /// <summary>
        /// Inserts a new category into the Categories table.
        /// INSERT(Name, IsActive, CreatedDate)
        /// </summary>
        public void InsertCategory(Category model)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"
INSERT INTO Categories (Name, IsActive, CreatedDate)
VALUES (@Name, @IsActive, @CreatedDate)
";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = model.Name ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = model.IsActive });
            command.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = model.CreatedDate });

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}

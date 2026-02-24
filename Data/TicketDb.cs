using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using HelpdeskSystem.Models;

namespace HelpdeskSystem.Data
{
    // Data access for Tickets and related operations
    public class TicketDb
    {
        private readonly Db _db;

        public TicketDb(Db db)
        {
            _db = db;
        }

        public void InsertTicket(Ticket model)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"INSERT INTO Tickets (Title, Description, CategoryId, CreatedBy, Status, CreatedDate, IsDeleted)
VALUES (@Title, @Description, @CategoryId, @CreatedBy, @Status, @CreatedDate, @IsDeleted)";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = model.Title ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1) { Value = model.Description ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = model.CategoryId });
            command.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = model.CreatedBy });
            command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = model.Status ?? string.Empty });
            command.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = model.CreatedDate });
            command.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = model.IsDeleted });

            connection.Open();
            command.ExecuteNonQuery();
        }

        public List<Category> GetActiveCategoriesForDropdown()
        {
            var list = new List<Category>();
            using var connection = _db.CreateConnection();
            const string sql = "SELECT Id, Name FROM Categories WHERE IsActive = 1 ORDER BY Name";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }

            return list;
        }

        public List<Category> GetAllCategoriesForFilter()
        {
            var list = new List<Category>();
            using var connection = _db.CreateConnection();
            const string sql = "SELECT Id, Name FROM Categories ORDER BY Name";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                });
            }

            return list;
        }

        public List<TicketListItem> GetTicketsPaged(string? search, string? status, int? categoryId, int page, int pageSize)
        {
            var list = new List<TicketListItem>();
            using var connection = _db.CreateConnection();

            var sql = new StringBuilder();
            sql.AppendLine("SELECT t.Id, t.Title, t.Status, c.Name AS CategoryName, t.CreatedDate, t.CreatedBy");
            sql.AppendLine("FROM Tickets t");
            sql.AppendLine("LEFT JOIN Categories c ON t.CategoryId = c.Id");
            sql.AppendLine("WHERE t.IsDeleted = 0");

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql.AppendLine("AND (t.Title LIKE @Search OR t.Description LIKE @Search)");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql.AppendLine("AND t.Status = @Status");
            }
            if (categoryId.HasValue)
            {
                sql.AppendLine("AND t.CategoryId = @CategoryId");
            }

            sql.AppendLine("ORDER BY t.CreatedDate DESC");
            sql.AppendLine("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

            using var command = new SqlCommand(sql.ToString(), connection) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(search))
            {
                command.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 4000) { Value = "%" + search + "%" });
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = status });
            }
            if (categoryId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId.Value });
            }

            var offset = (page - 1) * pageSize;
            command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
            command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TicketListItem
                {
                    Id = reader.GetInt32(0),
                    Title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Status = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    CategoryName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4)
                });
            }

            return list;
        }

        public int GetTicketsCount(string? search, string? status, int? categoryId)
        {
            using var connection = _db.CreateConnection();
            var sql = new StringBuilder();
            sql.AppendLine("SELECT COUNT(1) FROM Tickets t");
            sql.AppendLine("WHERE t.IsDeleted = 0");

            if (!string.IsNullOrWhiteSpace(search))
            {
                sql.AppendLine("AND (t.Title LIKE @Search OR t.Description LIKE @Search)");
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql.AppendLine("AND t.Status = @Status");
            }
            if (categoryId.HasValue)
            {
                sql.AppendLine("AND t.CategoryId = @CategoryId");
            }

            using var command = new SqlCommand(sql.ToString(), connection) { CommandType = CommandType.Text };

            if (!string.IsNullOrWhiteSpace(search))
            {
                command.Parameters.Add(new SqlParameter("@Search", SqlDbType.NVarChar, 4000) { Value = "%" + search + "%" });
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = status });
            }
            if (categoryId.HasValue)
            {
                command.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId.Value });
            }

            connection.Open();
            var result = (int)command.ExecuteScalar()!;
            return result;
        }

        public TicketDetailsViewModel? GetTicketDetails(int ticketId)
        {
            using var connection = _db.CreateConnection();
            const string sql = @"SELECT t.Id, t.Title, t.Description, t.Status, t.CreatedDate, c.Name
FROM Tickets t
LEFT JOIN Categories c ON t.CategoryId = c.Id
WHERE t.Id = @Id AND t.IsDeleted = 0";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = ticketId });

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new TicketDetailsViewModel
                {
                    TicketId = reader.GetInt32(0),
                    Title = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    Status = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    CreatedDate = reader.GetDateTime(4),
                    CategoryName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    Comments = new List<TicketDetailsViewModel.CommentItem>()
                };
            }

            return null;
        }

        public List<TicketDetailsViewModel.CommentItem> GetCommentsForTicket(int ticketId)
        {
            var list = new List<TicketDetailsViewModel.CommentItem>();
            using var connection = _db.CreateConnection();
            const string sql = @"SELECT tc.Id, tc.CommentText, tc.CreatedDate, u.FullName
FROM TicketComments tc
LEFT JOIN Users u ON tc.CreatedByU = u.Id
WHERE tc.TicketId = @TicketId
ORDER BY tc.CreatedDate ASC";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.Add(new SqlParameter("@TicketId", SqlDbType.Int) { Value = ticketId });

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TicketDetailsViewModel.CommentItem
                {
                    Id = reader.GetInt32(0),
                    CommentText = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    CreatedDate = reader.GetDateTime(2),
                    CreatedByName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3)
                });
            }

            return list;
        }

        public bool AddComment(int ticketId, string commentText, int createdByUserId)
        {
            using var connection = _db.CreateConnection();

            const string checkSql = "SELECT Status FROM Tickets WHERE Id = @Id AND IsDeleted = 0";
            using var checkCmd = new SqlCommand(checkSql, connection) { CommandType = CommandType.Text };
            checkCmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = ticketId });

            connection.Open();
            var statusObj = checkCmd.ExecuteScalar();
            if (statusObj == null || statusObj == DBNull.Value)
            {
                return false;
            }

            var status = statusObj.ToString() ?? string.Empty;
            if (string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            const string insertSql = @"INSERT INTO TicketComments (TicketId, CommentText, CreatedByU, CreatedDate)
VALUES (@TicketId, @CommentText, @CreatedByU, @CreatedDate)";
            using var insertCmd = new SqlCommand(insertSql, connection) { CommandType = CommandType.Text };
            insertCmd.Parameters.Add(new SqlParameter("@TicketId", SqlDbType.Int) { Value = ticketId });
            insertCmd.Parameters.Add(new SqlParameter("@CommentText", SqlDbType.NVarChar, -1) { Value = commentText ?? string.Empty });
            insertCmd.Parameters.Add(new SqlParameter("@CreatedByU", SqlDbType.Int) { Value = createdByUserId });
            insertCmd.Parameters.Add(new SqlParameter("@CreatedDate", SqlDbType.DateTime2) { Value = DateTime.Now });

            insertCmd.ExecuteNonQuery();
            return true;
        }

        public bool SoftDeleteTicket(int ticketId)
        {
            using var connection = _db.CreateConnection();
            const string sql = "UPDATE Tickets SET IsDeleted = 1 WHERE Id = @Id";
            using var command = new SqlCommand(sql, connection) { CommandType = CommandType.Text };
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = ticketId });

            connection.Open();
            var affected = command.ExecuteNonQuery();
            return affected > 0;
        }
    }
}

using HelpdeskSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HelpdeskSystem.Controllers
{
    public class TestController : Controller
    {
        private readonly Db _db;

        public TestController(Db db)
        {
            _db = db;
        }

        public IActionResult DbTest()
        {
            using var conn = _db.CreateConnection();
            using var cmd = new SqlCommand("SELECT 1", conn);

            conn.Open();
            var result = (int)cmd.ExecuteScalar();

            return Content($"DB OK: {result}");
        }
    }
}
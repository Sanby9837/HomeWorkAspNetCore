using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Server.Models.Entity;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersControllerDapper : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public UsersControllerDapper(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var users = connection.Query<Users>("SELECT * FROM Users");
                return Ok(users);
            }
        }

    }
}

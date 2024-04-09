using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server.Models.Entity;
using StackExchange.Redis;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersControllerDapper : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IConnectionMultiplexer _redis;

        public UsersControllerDapper(IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? throw new Exception("Dapper 連線異常");
            _redis = redis;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var db = _redis.GetDatabase(0);
            db.StringSet("username", "sanby");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var users = connection.Query<Users>("SELECT * FROM Users");
                return Ok(users);
            }
        }

        [HttpPost]
        public IActionResult CreateUser(Users user)
        {
            if (user == null)
            {
                return BadRequest("User data is required");
            }

            user.CreateDateTime = DateTime.Now;
            user.UpdateTime = DateTime.Now;

            //使用using 區塊來建立了一個 SqlConnection 對象，並開啟了資料庫連線。
            //定義了一個 SQL 查詢字串，用於向 Users 表中插入新的使用者記錄。
            //在執行 SQL 查詢時，我們使用了 Dapper 提供的 Execute 方法，並傳入了查詢字串和 user 物件作為參數。
            //Dapper 會自動將 user 物件中的屬性值填入 SQL 查詢字串中的參數中，並執行資料庫操作。
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Users (Name, CreateDateTime, UpdateTime) " +
                    "VALUES (@Name, @CreateDateTime, @UpdateTime)";
                //result = 執行操作後影響的行數
                var result = connection.Execute(query, user);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create user");
                }
            }
        }

        [HttpPut]
        public IActionResult UpdateUser([FromQuery] int id, Users user)
        {
            user.Id = id;
            if (user == null || user.Id != id)
            {
                return BadRequest("Invalid user data");
            }

            user.UpdateTime = DateTime.Now;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Users SET Name = @Name, UpdateTime = @UpdateTime WHERE Id = @Id";
                var result = connection.Execute(query, user);
                if (result > 0)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound($"User with Id {id} not found");
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Users WHERE Id = @Id";
                var result = connection.Execute(query, new { Id = id });
                if (result > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound($"User with Id {id} not found");
                }
            }
        }
    }
}

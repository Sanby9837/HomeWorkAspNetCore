using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Npgsql;
using Server.Models.Entity;
using System.Text.Json;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductControllerDapper : Controller
    {
        private readonly IConfiguration _configuraion;
        private readonly string _connectionString;

        public ProductControllerDapper(IConfiguration configuration)
        {
            _configuraion = configuration;
            _connectionString = _configuraion.GetConnectionString("PostgreDefaultConnection") ?? throw new Exception("Dapper 連線異常");
        }

        [HttpGet]
        public IActionResult GetProduct()
        {

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var Product = connection.Query<Product>("SELECT * FROM Product").AsList();
                return Ok(Product);
            }
        }

        [HttpPost]
        public IActionResult CreateUser(Product product)
        {
            if (product == null)
            {
                return BadRequest("product data is required");
            }

            //使用using 區塊來建立了一個 NpgsqlConnection 對象，並開啟了資料庫連線。
            //定義了一個 SQL 查詢字串，用於向 Product 表中插入新的使用者記錄。
            //在執行 SQL 查詢時，我們使用了 Dapper 提供的 Execute 方法，並傳入了查詢字串和 product 物件作為參數。
            //Dapper 會自動將 product 物件中的屬性值填入 SQL 查詢字串中的參數中，並執行資料庫操作。
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "INSERT INTO Product (Name, Price) " +
                    "VALUES (@Name, @Price)";
                //result = 執行操作後影響的行數
                var result = connection.Execute(query, product);
                if (result > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create product");
                }
            }
        }

        [HttpPut]
        public IActionResult UpdateUser([FromQuery] int id, Product product)
        {
            product.Id = id;
            if (product == null || product.Id != id)
            {
                return BadRequest("Invalid user data");
            }

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Product SET Name = @Name WHERE Id = @Id";
                var result = connection.Execute(query, product);
                if (result > 0)
                {
                    return Ok(product);
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Product WHERE Id = @Id";
                var result = connection.Execute(query, new { Id = id });
                if (result > 0)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound($"Product with Id {id} not found");
                }
            }
        }

    }
}

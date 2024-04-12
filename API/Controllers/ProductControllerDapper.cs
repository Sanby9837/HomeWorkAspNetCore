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
    }
}

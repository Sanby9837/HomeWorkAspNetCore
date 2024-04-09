

using API.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Server;
using Server.Models.Entity;
using System.Xml.Linq;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;
        public UsersController(ApplicationDbContext context,IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }
        

        [HttpGet]
        public IActionResult Get()
        {
            if(!_memoryCache.TryGetValue("UserD",out List<Users> cachedData))
            {
                //快取沒有data，抓資料庫的資料來放
                cachedData = _context.Users.ToList();
                //data存入快取過期時間設為5分鐘
                _memoryCache.Set("UserD", cachedData, TimeSpan.FromMinutes(1));
            }

            return Ok(cachedData);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var getData = _context.Users.FirstOrDefault(x => x.Id == id);

            if (getData == null)
            {
                return NotFound(id);
            }

            return Ok(getData);
        }

        [HttpGet("GetByName/{name}")]
        public IActionResult Get(string name)
        {

            var getDate = _context.Users.Where(x => x.Name == name)
                                        .OrderByDescending(x => x.Id)
                                        .ToList();

            if (getDate == null || !getDate.Any())
            {
                return NotFound(name);
            }
            return Ok(getDate);

        }

        [HttpPost]
        public IActionResult Create(UsersCreateRequest request)
        {
            var insertData = new Users()
            {
                Name = request.Name,
                CreateDateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
            };

            _context.Users.Add(insertData);

            _context.SaveChanges();

            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, UsersCreateRequest request)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound(id);
            }

            user.Name = request.Name;
            user.UpdateTime = DateTime.Now;

            _context.SaveChanges();

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            if (user == null)
            {
                return NotFound(id);
            }

            _context.Users.Remove(user);

            _context.SaveChanges();

            return Ok();
        }

    }
}

using AuthenService.Request;
using AuthenService.Service;
using JWT_Authen.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AuthenService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly Repository<Emp> _empRepository;
        private readonly TokenService _tokenService;
        private readonly RedisService _redisService;
        private const string CACHE_KEY_PREFIX = "emp:";
        public LoginController(MongoDbContext context,TokenService token, RedisService redisService)
        {
            _context = context;
            _empRepository = new Repository<Emp>(_context);
            _tokenService = token;
            _redisService = redisService;
        }
        
        [HttpPost]
        public async Task<IActionResult> Login(LoginReq emp)
        {

            var existingEmp = await _context.Emps.Find(e => e.EmailAddress == emp.Email && e.Password == emp.PassWord).FirstOrDefaultAsync();
            if (existingEmp == null)
            {
                return NotFound();
            }

            var userRole = await _context.UserRoles
                .Find(ur => ur.EmpId == existingEmp.ID)
                .FirstOrDefaultAsync();
            if (userRole == null)
            {
                return NotFound();
            }

            var roleFunctions = await _context.RolePermissions
                .Find(rf => rf.RoleId == userRole.RoleId)
                .ToListAsync();

            var functionCodes = new List<string>();
            foreach (var roleFunc in roleFunctions)
            {
                var function = await _context.Permissions.Find(f => f.ID == roleFunc.FuncId).FirstOrDefaultAsync();
                if (function != null)
                {
                    functionCodes.Add(function.Code);
                }
            }

            var empCache = new CacheRequest
            {
                Id = existingEmp.ID,
                Username = existingEmp.Name,
                Functions = functionCodes,
            };

            await _redisService.SetAsync($"{CACHE_KEY_PREFIX}{existingEmp.ID}", empCache, TimeSpan.FromHours(1));
            var token = _tokenService.GenerateToken(existingEmp.ID);

            return Ok(new { Token = token });
        }
    }
}

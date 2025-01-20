using AuthenService.Request;
using JWT_Authen.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AuthenService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Repository<Emp> _empRepository;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
            _empRepository = new Repository<Emp>(_context);
        }
        [HttpPost]
        public async Task<IActionResult> Login(EmpRequest emp)
        {
            var existingEmp = await _context.Emps.FindAsync(e => e.EmailAddress == emp.Email && e.Password == emp.PassWord);
            if (existingEmp == null)
            {
                return NotFound();
            }
            return (IActionResult)existingEmp;
        }
    }
}

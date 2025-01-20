using JWT_Authen.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicronserviceV2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly Repository<Emp> _empRepository;

        public EmpController(MongoDbContext context)
        {
            _context = context;
            _empRepository = new Repository<Emp>(_context);
        }

        [HttpGet]
        public async Task<IEnumerable<Emp>> GetEmps()
        {
            return await _empRepository.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Emp>> GetEmp(int id)
        {
            var emp = await _empRepository.GetByIdAsync(id);
            if (emp == null)
            {
                return NotFound();
            }
            return emp;
        }

        [HttpPost]
        public async Task<ActionResult<Emp>> CreateEmp(Emp emp)
        {
            await _empRepository.AddAsync(emp);
            return CreatedAtAction(nameof(GetEmp), new { id = emp.ID }, emp);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmp(int id, Emp emp)
        {
            if (id != emp.ID)
            {
                return BadRequest();
            }

            var existingEmp = await _empRepository.GetByIdAsync(id);
            if (existingEmp == null)
            {
                return NotFound();
            }

            await _empRepository.UpdateAsync(emp);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmp(int id)
        {
            var emp = await _empRepository.GetByIdAsync(id);
            if (emp == null)
            {
                return NotFound();
            }

            await _empRepository.DeleteAsync(emp);
            return NoContent();
        }
    }
}

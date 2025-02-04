using EmpService.Modes;
using JWT_Authen.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
        public async Task<IEnumerable<EmpRequest>> GetEmps()
        {
            var emps = await _empRepository.GetAllAsync();
            var empDtos = new List<EmpRequest>();

            foreach (var emp in emps)
            {
                empDtos.Add(new EmpRequest
                {
                    ID = emp.ID.ToString(), // Convert ObjectId to string
                    PhoneNumber = emp.PhoneNumber,
                    EmailAddress = emp.EmailAddress,
                    Name = emp.Name,
                    Password = emp.Password,                   
                });
            }

            return empDtos;
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
        public async Task<ActionResult<Emp>> CreateEmp(EmpRequest emp)
        {
            if (emp == null)
            {
                return BadRequest();
            }
            var emps = new Emp
            {
                ObjectId = ObjectId.GenerateNewId(), // Fix: Generate a new ObjectId
                EmailAddress = emp.EmailAddress,
                Name = emp.Name,
                Password = emp.Password,
                PhoneNumber = emp.PhoneNumber,
            };
            await _empRepository.AddAsync(emps);
            return CreatedAtAction(nameof(GetEmp), new { id = emps.ID }, emps);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmp(int id, EmpRequest emp)
        {
            // Parse the string id to ObjectId

            var existingEmp = await _empRepository.GetByIdAsync(id);
            if (existingEmp == null)
            {
                return NotFound();
            }

            // Update the existingEmp properties with the values from emp
            existingEmp.EmailAddress = emp.EmailAddress;
            existingEmp.Name = emp.Name;
            existingEmp.Password = emp.Password;
            existingEmp.PhoneNumber = emp.PhoneNumber;

            await _empRepository.UpdateAsync(existingEmp);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmp(int id)
        {
             // Parse the string id to ObjectId
            var emp = await _empRepository.GetByIdAsync(id);
            if (emp == null)
            {
                return NotFound();
            }

            await _empRepository.DeleteAsync(emp); // Pass the ObjectId to DeleteAsync
            return NoContent();
        }
    }
}

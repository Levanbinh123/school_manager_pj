using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    //[Authorize(Roles = "Admin")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentsController(
            IStudentService studentService)
        {
            _studentService = studentService;
        }

        // =====================================================
        // GET ALL
        // GET: api/admin/Students
        // =====================================================

        [HttpGet]
        public IActionResult GetAllStudents(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null)
        {
            var students =
                _studentService.GetAll(
                    pageNumber,
                    pageSize,
                    search);

            return Ok(students);
        }

        // =====================================================
        // GET BY ID
        // GET: api/admin/Students/1
        // =====================================================

        [HttpGet("{id}")]
        public IActionResult GetStudentById(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Student ID không hợp lệ"
                });
            }

            var student =
                _studentService.GetById(id);

            if (student == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy Student"
                });
            }

            return Ok(student);
        }

        // =====================================================
        // COUNT
        // GET: api/admin/Students/count
        // =====================================================

        [HttpGet("count")]
        public IActionResult GetStudentCount()
        {
            var count =
                _studentService.GetAllStudents();

            return Ok(new
            {
                totalStudents = count
            });
        }

        // =====================================================
        // ADD
        // POST: api/admin/Students
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> AddStudent(
            [FromBody] CreateStudentViewModel student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _studentService
                    .AddStudent(student);

                return Ok(new
                {
                    message = "Student added successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerException =
                        ex.InnerException?.Message,
                    innerInnerException =
                        ex.InnerException?
                            .InnerException?.Message
                });
            }
        }

        // =====================================================
        // UPDATE
        // PUT: api/admin/Students/1
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(
            int id,
            [FromBody] CreateStudentViewModel student)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Student ID không hợp lệ"
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _studentService
                    .UpdateStudent(
                        id,
                        student);

                return Ok(new
                {
                    message = "Student updated successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE
        // DELETE: api/admin/Students/1
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Student ID không hợp lệ"
                });
            }

            try
            {
                await _studentService
                    .DeleteStudent(id);

                return Ok(new
                {
                    message = "Student deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
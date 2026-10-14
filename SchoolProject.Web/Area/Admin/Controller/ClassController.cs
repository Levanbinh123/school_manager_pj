using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        // GET: api/admin/class
        [HttpGet]
        public async Task<IActionResult> GetAllClass()
        {
            var result = await _classService.GetAllClass();

            return Ok(result);
        }

        // GET: api/admin/class/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            var result = await _classService.GetClassById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Class not found"
                });
            }

            return Ok(result);
        }

        // POST: api/admin/class
        [HttpPost]
        public async Task<IActionResult> AddClass(
            [FromBody] CreateClassViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _classService.AddClass(vm);

                return Ok(new
                {
                    message = "Class created successfully"
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

        // PUT: api/admin/class/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClass(
            int id,
            [FromBody] CreateClassViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _classService.UpdateClass(id, vm);

                return Ok(new
                {
                    message = "Class updated successfully"
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

        // DELETE: api/admin/class/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                await _classService.DeleteClass(id);

                return Ok(new
                {
                    message = "Class deleted successfully"
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

        // POST: api/admin/class/session
        [HttpPost("session")]
        public async Task<IActionResult> AddClassSession(
            [FromBody] CreateClassSessionViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _classService.AddClassSession(vm);

                return Ok(new
                {
                    message = "ClassSession created successfully",
                    data = result
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

        // GET: api/admin/class/1/session
        [HttpGet("{classId}/session")]
        public async Task<IActionResult> GetClassSessions(int classId)
        {
            try
            {
                var result =
                    await _classService.GetClassSessions(classId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/admin/class/students
        [HttpPost("students")]
        public async Task<IActionResult> AddStudentsToClassSession(
            [FromBody] AddStudentsToClassSessionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var count =
                    await _classService.AddStudentsToClassSession(
                        request.ClassSessionId,
                        request.StudentList
                    );

                return Ok(new
                {
                    message = "Thêm Student vào ClassSession thành công",
                    count = count
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

        public class AddStudentsToClassSessionRequest
        {
            public int ClassSessionId { get; set; }

            public List<int> StudentList { get; set; }
                = new List<int>();
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(
            ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }

        // =====================================================
        // GET ALL TEACHERS
        // GET: api/admin/teacher?pageNumber=1&pageSize=10
        // =====================================================

        [HttpGet]
        public IActionResult GetAllTeacher(
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber <= 0)
            {
                return BadRequest(new
                {
                    message = "Page number không hợp lệ."
                });
            }

            if (pageSize <= 0)
            {
                return BadRequest(new
                {
                    message = "Page size không hợp lệ."
                });
            }

            var result =
                _teacherService.GetAllTeacher(
                    pageNumber,
                    pageSize);

            return Ok(result);
        }

        // =====================================================
        // GET TEACHER COUNT
        // GET: api/admin/teacher/count
        // =====================================================

        [HttpGet("count")]
        public IActionResult GetTeacherCount()
        {
            var count =
                _teacherService.GetAllTeacher();

            return Ok(new
            {
                totalTeachers = count
            });
        }

        // =====================================================
        // ADD TEACHER
        // POST: api/admin/teacher
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> AddTeacher(
            [FromBody] CreateTeacherViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _teacherService.AddTeacher(vm);

                return Ok(new
                {
                    message =
                        "Teacher created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message,
                    innerException =
                        ex.InnerException?.Message
                });
            }
        }

        // =====================================================
        // ADD TEACHERS TO SESSION
        //
        // POST:
        // api/admin/teacher/session?sessionId=1
        //
        // Body:
        // [
        //     1,
        //     2,
        //     3
        // ]
        // =====================================================

        [HttpPost("session")]
        public async Task<IActionResult> AddSessionWithTeacher(
            [FromQuery] int sessionId,
            [FromBody] List<int> teacherIdsList)
        {
            if (sessionId <= 0)
            {
                return BadRequest(new
                {
                    message = "Session ID không hợp lệ."
                });
            }

            if (teacherIdsList == null ||
                teacherIdsList.Count == 0)
            {
                return BadRequest(new
                {
                    message =
                        "Danh sách Teacher không được để trống."
                });
            }

            if (teacherIdsList.Any(x => x <= 0))
            {
                return BadRequest(new
                {
                    message =
                        "Teacher ID không hợp lệ."
                });
            }

            try
            {
                var count =
                    await _teacherService
                        .AddSessionWithTeacher(
                            sessionId,
                            teacherIdsList);

                return Ok(new
                {
                    message =
                        "Teachers added to session successfully",

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
    }
}
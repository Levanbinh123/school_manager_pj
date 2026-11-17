





using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Controllers
{
    [ApiController]
    [Route("api/teacher/scores")]
    [Authorize(Roles = "Teacher")]
    public class TeacherScoreController : ControllerBase
    {
        private readonly IStudentScoreService _studentScoreService;

        public TeacherScoreController(
            IStudentScoreService studentScoreService)
        {
            _studentScoreService = studentScoreService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(
                ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "Token không hợp lệ hoặc thiếu UserId.");
        }
        // ============================================================
        // POST: api/teacher/scores/class-session/5/calculate
        // Tính điểm toàn bộ sinh viên của một lớp học phần
        // ============================================================

        [HttpPost("class-session/{classSessionId:int}/calculate")]
        public async Task<IActionResult> CalculateClassScores(
            int classSessionId)
        {
            try
            {
                if (classSessionId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "ClassSessionId không hợp lệ."
                    });
                }

                var result = await _studentScoreService.CalculateClassScores(
                    GetUserId(),
                    classSessionId);

                return Ok(new
                {
                    message = $"Đã tính và chốt điểm cho {result.Count} sinh viên.",
                    totalStudents = result.Count,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        // ============================================================
        // GET: api/teacher/scores/class-session/5/students
        // Lấy toàn bộ sinh viên và điểm của lớp học phần
        // ============================================================

        [HttpGet("class-session/{classSessionId:int}/students")]
        public async Task<IActionResult> GetClassSessionStudents(
            int classSessionId)
        {
            try
            {
                if (classSessionId <= 0)
                {
                    return BadRequest(new
                    {
                        message = "ClassSessionId không hợp lệ."
                    });
                }

                var result = await _studentScoreService
                    .GetClassSessionStudents(
                        GetUserId(),
                        classSessionId);

                return Ok(new
                {
                    totalStudents = result.Count,
                    data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        // GET: api/teacher/scores/eligible-enrolls
        [HttpGet("eligible-enrolls")]
        public async Task<IActionResult> GetEligibleEnrolls()
        {
            try
            {
                var result = await _studentScoreService
                    .GetEligibleEnrollsForTeacher(GetUserId());

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
        // GET: api/teacher/scores
        // Xem tất cả điểm của các lớp giáo viên phụ trách
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _studentScoreService
                    .GetTeacherScores(GetUserId());

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // POST: api/teacher/scores
        // Tổng kết điểm cho một sinh viên
        [HttpPost]
        public async Task<IActionResult> Add(
            [FromBody] CreateStudentScoreViewModel vm)
        {
            try
            {
                if (vm == null)
                    return BadRequest(new
                    {
                        message = "Dữ liệu không hợp lệ."
                    });

                await _studentScoreService.AddScore(
                    GetUserId(), vm);

                return Ok(new
                {
                    message = "Tổng kết điểm thành công."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // PUT: api/teacher/scores/1
        // Cập nhật điểm tổng kết
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateStudentScoreViewModel vm)
        {
            try
            {
                if (id <= 0 || vm == null)
                    return BadRequest(new
                    {
                        message = "Dữ liệu không hợp lệ."
                    });

                await _studentScoreService.UpdateScore(
                    GetUserId(), id, vm);

                return Ok(new
                {
                    message = "Cập nhật điểm thành công."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
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

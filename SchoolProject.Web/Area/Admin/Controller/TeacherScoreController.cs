





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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

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
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    // GET: api/teacher/scores
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = GetUserId();

            var result = await _studentScoreService
                .GetTeacherScores(userId);

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

    // POST: api/teacher/scores
    // Chỉ tạo/finalize StudentScore sau khi Session.End
    [HttpPost]
    public async Task<IActionResult> Add(
        [FromBody] CreateStudentScoreViewModel vm)
    {
        try
        {
            var userId = GetUserId();

            await _studentScoreService.AddScore(userId, vm);

            return Ok(new
            {
                message = "Tạo điểm tổng kết thành công."
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

    // PUT: api/teacher/scores/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateStudentScoreViewModel vm)
    {
        try
        {
            var userId = GetUserId();

            await _studentScoreService
                .UpdateScore(userId, id, vm);

            return Ok(new
            {
                message = "Cập nhật điểm tổng kết thành công."
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
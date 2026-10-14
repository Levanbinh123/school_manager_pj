using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using System.Security.Claims;

[ApiController]
[Route("api/student/scores")]
[Authorize(Roles = "Student")]
public class StudentScoreController : ControllerBase
{
    private readonly IStudentScoreService _studentScoreService;

    public StudentScoreController(
        IStudentScoreService studentScoreService)
    {
        _studentScoreService = studentScoreService;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException();
    }

    // GET: api/student/scores
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = GetUserId();

            var result = await _studentScoreService
                .GetStudentScores(userId);

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

    // GET: api/student/scores/subject/1
    [HttpGet("subject/{subjectId}")]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        try
        {
            var userId = GetUserId();

            var result = await _studentScoreService
                .GetStudentScoresBySubject(userId, subjectId);

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
}
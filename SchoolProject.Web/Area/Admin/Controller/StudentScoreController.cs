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


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _studentScoreService
                .GetStudentScores(GetUserId());

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

    [HttpGet("subject/{subjectId:int}")]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        try
        {
            if (subjectId <= 0)
                return BadRequest(new
                {
                    message = "SubjectId không hợp lệ."
                });

            var result = await _studentScoreService
                .GetStudentScoresBySubject(
                    GetUserId(), subjectId);

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
}
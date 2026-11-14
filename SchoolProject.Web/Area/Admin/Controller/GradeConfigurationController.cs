using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [Route("api/teacher/grade-configurations")]
    [ApiController]
    [Authorize(Roles = "Teacher")]
    public class GradeConfigurationController : ControllerBase
    {
        private readonly IGradeConfigurationService _service;

        public GradeConfigurationController(
            IGradeConfigurationService service)
        {
            _service = service;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(
                ClaimTypes.NameIdentifier);
        }

        // GET: api/teacher/grade-configurations/class-session/15
        [HttpGet("class-session/{classSessionId:int}")]
        public async Task<IActionResult> GetByClassSession(
            int classSessionId)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _service.GetByClassSession(
                    userId, classSessionId);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/teacher/grade-configurations
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateGradeConfigurationViewModel vm)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _service.Create(userId, vm);

                return CreatedAtAction(
                    nameof(GetByClassSession),
                    new { classSessionId = result.ClassSessionId },
                    result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/teacher/grade-configurations/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateGradeConfigurationViewModel vm)
        {
            try
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized();

                var result = await _service.Update(
                    userId, id, vm);

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

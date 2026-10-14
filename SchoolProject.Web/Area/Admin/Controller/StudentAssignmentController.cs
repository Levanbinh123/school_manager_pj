using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/student/assignments")]
    [Authorize(Roles = "Student")]
    public class StudentAssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public StudentAssignmentController(
            IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        // =====================================================
        // GET ALL ASSIGNMENTS
        // =====================================================

        private string GetUserId()
        {
            return User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetStudentAssignments(GetUserId());

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


        // =====================================================
        // GET ASSIGNMENT BY ID
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetStudentAssignmentById(
                            GetUserId(),
                            id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Assignment"
                    });
                }

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


        // =====================================================
        // SUBMIT ASSIGNMENT
        // =====================================================

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(
            int id,
            [FromForm] SubmitAssignmentRequest request)
        {
            try
            {
                await _assignmentService.SubmitAssignment(
                    GetUserId(),
                    id,
                    request);

                return Ok(new
                {
                    message = "Nộp bài thành công"
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
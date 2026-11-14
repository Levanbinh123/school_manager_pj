using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/teacher/assignments")]
    [Authorize(Roles = "Teacher")]
    public class TeacherAssignmentController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public TeacherAssignmentController(
            IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        private string GetUserId()
        {
            return User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException();
        }


        // =====================================================
        // PING
        // =====================================================

        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("TeacherAssignmentController OK");
        }


        // =====================================================
        // ADD ASSIGNMENT
        // =====================================================
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Add(
            [FromForm] CreateAssignmentWithFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AssignmentJson))
                {
                    return BadRequest(new
                    {
                        message = "Thông tin Assignment không được để trống."
                    });
                }

                var vm = JsonSerializer.Deserialize<CreateAssignmentViewModel>(
                    request.AssignmentJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (vm == null)
                {
                    return BadRequest(new
                    {
                        message = "Dữ liệu Assignment không hợp lệ."
                    });
                }

                if (!TryValidateModel(vm))
                {
                    return BadRequest(ModelState);
                }

                await _assignmentService.AddAssignment(
                    GetUserId(),
                    vm,
                    request.File);

                return Ok(new
                {
                    message = "Tạo Assignment thành công"
                });
            }
            catch (JsonException)
            {
                return BadRequest(new
                {
                    message = "JSON Assignment không hợp lệ."
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
        // GET ALL ASSIGNMENTS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetTeacherAssignments(
                            GetUserId());

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
                        .GetTeacherAssignmentById(
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
        // UPDATE ASSIGNMENT
        // =====================================================

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(
          int id,
          [FromForm] CreateAssignmentWithFileRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.AssignmentJson))
                {
                    return BadRequest(new
                    {
                        message = "Thông tin Assignment không được để trống."
                    });
                }

                var vm = JsonSerializer.Deserialize<CreateAssignmentViewModel>(
                    request.AssignmentJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (vm == null)
                {
                    return BadRequest(new
                    {
                        message = "Dữ liệu Assignment không hợp lệ."
                    });
                }

                if (!TryValidateModel(vm))
                {
                    return BadRequest(ModelState);
                }

                await _assignmentService.UpdateAssignment(
                    GetUserId(),
                    id,
                    vm,
                    request.File);

                return Ok(new
                {
                    message = "Cập nhật Assignment thành công"
                });
            }
            catch (JsonException)
            {
                return BadRequest(new
                {
                    message = "JSON Assignment không hợp lệ."
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
        // DELETE ASSIGNMENT
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _assignmentService.DeleteAssignment(
                    GetUserId(),
                    id);

                return Ok(new
                {
                    message = "Xóa Assignment thành công"
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
        // GET SUBMISSIONS
        // =====================================================

        [HttpGet("{id}/submissions")]
        public async Task<IActionResult> GetSubmissions(
            int id)
        {
            try
            {
                var result =
                    await _assignmentService.GetSubmissions(
                        GetUserId(),
                        id);

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
        // GRADE SUBMISSION
        // =====================================================

        [HttpPut("/api/teacher/submissions/{id}/grade")]
        public async Task<IActionResult> Grade(
            int id,
            [FromBody] GradeSubmissionRequest request)
        {
            try
            {
                await _assignmentService.GradeSubmission(
                    GetUserId(),
                    id,
                    request);

                return Ok(new
                {
                    message = "Chấm điểm thành công"
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
        // GET TEACHER CLASS SESSIONS
        // =====================================================

        [HttpGet("class-sessions")]
        public async Task<IActionResult> GetClassSessions()
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetTeacherClassSessions(
                            GetUserId());

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
        // GET STUDENTS OF CLASS SESSION
        // =====================================================

        [HttpGet("class-sessions/{classSessionId}/students")]
        public async Task<IActionResult> GetClassSessionStudents(
            int classSessionId)
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetStudentsByClassSession(
                            GetUserId(),
                            classSessionId);

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
        // ASSIGNMENT SUMMARY
        // =====================================================

        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(
            int id)
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetAssignmentSummary(
                            GetUserId(),
                            id);

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
        // STUDENTS NOT SUBMITTED
        // =====================================================

        [HttpGet("{id}/not-submitted")]
        public async Task<IActionResult> GetNotSubmitted(
            int id)
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetNotSubmittedStudents(
                            GetUserId(),
                            id);

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
        // SUBMISSIONS NOT GRADED
        // =====================================================

        [HttpGet("{id}/not-graded")]
        public async Task<IActionResult> GetNotGraded(
            int id)
        {
            try
            {
                var result =
                    await _assignmentService
                        .GetNotGradedSubmissions(
                            GetUserId(),
                            id);

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
}
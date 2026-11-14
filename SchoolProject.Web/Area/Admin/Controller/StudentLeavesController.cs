using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StudentLeavesController : ControllerBase
    {
        private readonly IStudentLeaveService _studentLeaveService;

        public StudentLeavesController(
            IStudentLeaveService studentLeaveService)
        {
            _studentLeaveService = studentLeaveService;
        }

        // ==========================================
        // GET ALL LEAVE REQUESTS
        // GET: api/admin/StudentLeaves
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _studentLeaveService.GetAll();

            return Ok(result);
        }

        // ==========================================
        // GET BY ID
        // GET: api/admin/StudentLeaves/1
        // ==========================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Leave ID không hợp lệ."
                });

            var result = await _studentLeaveService.GetById(id);

            if (result == null)
                return NotFound(new
                {
                    message = "Không tìm thấy đơn bảo lưu."
                });

            return Ok(result);
        }

        // ==========================================
        // CREATE LEAVE REQUEST
        // POST: api/admin/StudentLeaves/student/1
        // ==========================================
        [HttpPost("student/{studentId:int}")]
        public async Task<IActionResult> Create(
            int studentId,
            [FromBody] CreateStudentLeaveViewModel model)
        {
            if (studentId <= 0)
                return BadRequest(new
                {
                    message = "Student ID không hợp lệ."
                });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _studentLeaveService.CreateLeave(
                    studentId,
                    model
                );

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ==========================================
        // APPROVE
        // PUT: api/admin/StudentLeaves/1/approve
        // ==========================================
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(
            int id,
            [FromBody] StudentLeaveDecisionViewModel model)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Leave ID không hợp lệ."
                });

            try
            {
                var approvedBy = User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

                await _studentLeaveService.Approve(
                    id,
                    approvedBy,
                    model
                );

                return Ok(new
                {
                    message = "Đã duyệt đơn bảo lưu thành công."
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

        // ==========================================
        // REJECT
        // PUT: api/admin/StudentLeaves/1/reject
        // ==========================================
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(
            int id,
            [FromBody] StudentLeaveDecisionViewModel model)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Leave ID không hợp lệ."
                });

            try
            {
                await _studentLeaveService.Reject(id, model);

                return Ok(new
                {
                    message = "Đã từ chối đơn bảo lưu."
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

        // ==========================================
        // RETURN TO STUDY
        // PUT: api/admin/StudentLeaves/1/return
        // ==========================================
        [HttpPut("{id:int}/return")]
        public async Task<IActionResult> ReturnToStudy(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Leave ID không hợp lệ."
                });

            try
            {
                await _studentLeaveService.ReturnToStudy(id);

                return Ok(new
                {
                    message = "Sinh viên đã được xác nhận quay lại học."
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

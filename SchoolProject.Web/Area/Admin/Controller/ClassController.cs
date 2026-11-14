
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        // GET: api/admin/class
        [HttpGet]
        public async Task<IActionResult> GetAllClass()
        {
            var result = await _classService.GetAllClass();
            return Ok(result);
        }

        // GET: api/admin/class/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetClassById(int id)
        {
            var result = await _classService.GetClassById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Class not found"
                });
            }

            return Ok(result);
        }

        // POST: api/admin/class
        [HttpPost]
        public async Task<IActionResult> AddClass(
            [FromBody] CreateClassViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _classService.AddClass(vm);

                return Ok(new
                {
                    message = "Class created successfully"
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

        // PUT: api/admin/class/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateClass(
            int id,
            [FromBody] CreateClassViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _classService.UpdateClass(id, vm);

                return Ok(new
                {
                    message = "Class updated successfully"
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

        // DELETE: api/admin/class/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                await _classService.DeleteClass(id);

                return Ok(new
                {
                    message = "Class deleted successfully"
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

        // POST: api/admin/class/session
        [HttpPost("session")]
        public async Task<IActionResult> AddClassSession(
            [FromBody] CreateClassSessionViewModel vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _classService.AddClassSession(vm);

                return Ok(new
                {
                    message = "ClassSession created successfully",
                    data = result
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

        // GET: api/admin/class/1/session
        [HttpGet("{classId:int}/session")]
        public async Task<IActionResult> GetClassSessions(int classId)
        {
            try
            {
                var result =
                    await _classService.GetClassSessions(classId);

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

        // GET: api/admin/class/sessions
        [HttpGet("sessions")]
        public async Task<IActionResult> GetAllClassSessions()
        {
            try
            {
                var result =
                    await _classService.GetAllClassSessions();

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

        // POST: api/admin/class/students
        [HttpPost("students")]
        public async Task<IActionResult> AddStudentsToClassSession(
            [FromBody] AddStudentsToClassSessionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.ClassSessionId <= 0)
            {
                return BadRequest(new
                {
                    message = "ClassSessionId không hợp lệ."
                });
            }

            if (request.StudentList == null ||
                request.StudentList.Count == 0)
            {
                return BadRequest(new
                {
                    message = "Danh sách Student không được để trống."
                });
            }

            try
            {
                var count =
                    await _classService.AddStudentsToClassSession(
                        request.ClassSessionId,
                        request.StudentList);

                return Ok(new
                {
                    message = "Thêm Student vào ClassSession thành công.",
                    count
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

        // POST: api/admin/class/session/1/confirm
        [HttpPost("session/{id:int}/confirm")]
        public async Task<IActionResult> ConfirmClassSession(int id)
        {
            try
            {
                await _classService.ConfirmClassSession(id);

                return Ok(new
                {
                    message = "Đã xác nhận lớp học phần thành công."
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

        // POST: api/admin/class/session/1/cancel
        [HttpPost("session/{id:int}/cancel")]
        public async Task<IActionResult> CancelClassSession(int id)
        {
            try
            {
                await _classService.CancelClassSession(id);

                return Ok(new
                {
                    message = "Đã hủy lớp học phần thành công."
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

        public class AddStudentsToClassSessionRequest
        {
            public int ClassSessionId { get; set; }

            public List<int> StudentList { get; set; }
                = new List<int>();
        }
        // =====================================================
        // 1. GET CLASSES WAITING FOR PAYMENT APPROVAL
        // GET: api/admin/class/payment/waiting
        // =====================================================
        [HttpGet("payment/waiting")]
        public async Task<IActionResult> GetClassesAwaitingPaymentApproval()
        {
            try
            {
                var result =
                    await _classService.GetClassesAwaitingPaymentApproval();

                return Ok(new
                {
                    message = "Danh sách lớp chờ duyệt thanh toán.",
                    total = result.Count,
                    data = result
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
        // 2. ADMIN APPROVES PAYMENT PERIOD
        // POST: api/admin/class/session/1/payment/approve
        // =====================================================
        [HttpPost("session/{id:int}/payment/approve")]
        public async Task<IActionResult> ApprovePaymentPeriod(int id)
        {
            try
            {
                await _classService.ApprovePaymentPeriod(id);

                return Ok(new
                {
                    message = "Đã mở đợt thanh toán thành công.",
                    classSessionId = id,
                    paymentDurationDays = 7
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
        // 3. GET PAYMENT SUMMARY
        // GET: api/admin/class/session/1/payment/summary
        // =====================================================
        [HttpGet("session/{id:int}/payment/summary")]
        public async Task<IActionResult> GetPaymentSummary(int id)
        {
            try
            {
                var result =
                    await _classService.GetPaymentSummary(id);

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

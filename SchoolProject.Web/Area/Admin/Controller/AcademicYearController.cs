using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies.Academiyear;
using SchoolProject.ViewModels;
using SchoolProject.ViewModels.AcademicYear;
using SchoolProject.ViewModels.AcademicYear.SchoolProject.ViewModels;
using System.Security.Claims;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [Route("api/admin/academic-years")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AcademicYearController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearController(
            IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        // GET: api/admin/academic-years
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _academicYearService.GetAll();
            return Ok(result);
        }

        // GET: api/admin/academic-years/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _academicYearService.GetById(id);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Không tìm thấy năm học."
                });
            }

            return Ok(result);
        }

        // GET: api/admin/academic-years/current
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrent()
        {
            var result = await _academicYearService.GetCurrent();

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Hiện tại chưa có năm học nào đang hoạt động."
                });
            }

            return Ok(result);
        }

        // POST: api/admin/academic-years
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAcademicYearViewModel vm)
        {
            try
            {
                var result = await _academicYearService.Create(vm);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/academic-years/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateAcademicYearViewModel vm)
        {
            try
            {
                var result = await _academicYearService.Update(id, vm);

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/academic-years/1/activate
        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _academicYearService.Activate(id);

                return Ok(new
                {
                    message = "Kích hoạt năm học thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/academic-years/1/complete
        [HttpPut("{id:int}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            try
            {
                await _academicYearService.Complete(id);

                return Ok(new
                {
                    message = "Hoàn thành năm học thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/admin/academic-years/1/close

        [HttpPut("{id:int}/close")]
        public async Task<IActionResult> Close(
            int id,
            [FromBody] CloseAcademicYearViewModel vm)
        {
            try
            {
                // Lấy ID Admin từ JWT
                var adminUserId =
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrWhiteSpace(adminUserId))
                {
                    return Unauthorized(new
                    {
                        message = "Không xác định được tài khoản Admin."
                    });
                }

                await _academicYearService.Close(
                    id,
                    vm,
                    adminUserId);

                return Ok(new
                {
                    message = "Đóng năm học thành công."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Servies;
using SchoolProject.ViewModels;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/fees")]
    [Authorize(Roles = "Admin")]
    public class FeeController : ControllerBase
    {
        private readonly IFeeService _feeService;

        public FeeController(IFeeService feeService)
        {
            _feeService = feeService;
        }

        // =====================================================
        // GET ALL FEES
        // GET: api/admin/fees
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _feeService.GetAllFees();

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
        // GET FEE BY ID
        // GET: api/admin/fees/{id}
        // =====================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _feeService.GetFeeById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy học phí."
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
        // CREATE FEE
        // POST: api/admin/fees
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateFeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result =
                    await _feeService.CreateFee(vm);

                return Ok(new
                {
                    message = "Tạo học phí thành công.",
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
        // UPDATE FEE
        // PUT: api/admin/fees/{id}
        // =====================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateFeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result =
                    await _feeService.UpdateFee(id, vm);

                return Ok(new
                {
                    message = "Cập nhật học phí thành công.",
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
        // DELETE FEE
        // DELETE: api/admin/fees/{id}
        // =====================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result =
                    await _feeService.DeleteFee(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy học phí."
                    });
                }

                return Ok(new
                {
                    message = "Xóa học phí thành công."
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
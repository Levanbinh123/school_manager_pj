using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolProject.Models;
using SchoolProject.Servies;

namespace SchoolProject.Web.Area.Admin.Controller
{
    [ApiController]
    [Route("api/admin/study-periods")]
    public class StudyPeriodController : ControllerBase
    {
        private readonly IStudyPeriodService _studyPeriodService;

        public StudyPeriodController(
            IStudyPeriodService studyPeriodService)
        {
            _studyPeriodService = studyPeriodService;
        }

        // =====================================================
        // GET ALL
        // GET: api/admin/study-periods
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _studyPeriodService.GetAll();

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
        // GET ACTIVE PERIODS
        // GET: api/admin/study-periods/active
        // =====================================================
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePeriods()
        {
            try
            {
                var result =
                    await _studyPeriodService.GetActivePeriods();

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
        // GET BY ID
        // GET: api/admin/study-periods/1
        // =====================================================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _studyPeriodService.GetById(id);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy tiết học."
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
        // CREATE
        // POST: api/admin/study-periods
        // =====================================================
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] StudyPeriod model)
        {
            try
            {
                await _studyPeriodService.Add(model);

                return Ok(new
                {
                    message = "Thêm tiết học thành công."
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
        // UPDATE
        // PUT: api/admin/study-periods/1
        // =====================================================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] StudyPeriod model)
        {
            try
            {
                await _studyPeriodService.Update(id, model);

                return Ok(new
                {
                    message = "Cập nhật tiết học thành công."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Không tìm thấy tiết học.")
                {
                    return NotFound(new
                    {
                        message = ex.Message
                    });
                }

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE
        // DELETE: api/admin/study-periods/1
        // =====================================================
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _studyPeriodService.Delete(id);

                return Ok(new
                {
                    message = "Xóa tiết học thành công."
                });
            }
            catch (Exception ex)
            {
                if (ex.Message == "Không tìm thấy tiết học.")
                {
                    return NotFound(new
                    {
                        message = ex.Message
                    });
                }

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}

//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SchoolProject.Servies;
//using SchoolProject.ViewModels;

//namespace SchoolProject.Web.Controllers
//{
//    [ApiController]
//    [Route("api/admin/[controller]")]
//    [Authorize(Roles = "Admin")]
//    public class GradesController : ControllerBase
//    {
//        private readonly IGradeService _gradeService;

//        public GradesController(IGradeService gradeService)
//        {
//            _gradeService = gradeService;
//        }

//        // =====================================================
//        // GET: api/admin/Grades?pageNumber=1&pageSize=10
//        // =====================================================

//        [HttpGet]
//        public IActionResult GetAll(
//            int pageNumber = 1,
//            int pageSize = 10)
//        {
//            var result =
//                _gradeService.GetAll(
//                    pageNumber,
//                    pageSize);

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: api/admin/Grades/all
//        // =====================================================

//        [HttpGet("all")]
//        public IActionResult GetAll()
//        {
//            var result = _gradeService.GetAll();

//            return Ok(result);
//        }

//        // =====================================================
//        // GET: api/admin/Grades/5
//        // =====================================================

//        [HttpGet("{gradeId}")]
//        public async Task<IActionResult> GetById(int gradeId)
//        {
//            var grade =
//                await _gradeService.GetById(gradeId);

//            if (grade == null)
//            {
//                return NotFound(new
//                {
//                    message = "Không tìm thấy Grade"
//                });
//            }

//            return Ok(grade);
//        }

//        // =====================================================
//        // POST: api/admin/Grades
//        // =====================================================

//        [HttpPost]
//        public async Task<IActionResult> Add(
//            [FromBody] CreateGradeViewModel vm)
//        {
//            if (!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }

//            try
//            {
//                var result =
//                    await _gradeService.Add(vm);

//                return Ok(new
//                {
//                    message = "Thêm Grade thành công",
//                    data = result
//                });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest(new
//                {
//                    message = ex.Message
//                });
//            }
//        }
//    }
//}
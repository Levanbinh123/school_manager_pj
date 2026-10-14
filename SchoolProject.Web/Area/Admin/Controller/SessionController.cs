    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SchoolProject.Servies;
    using SchoolProject.ViewModels;

    namespace SchoolProject.Web.Controllers
    {
        [ApiController]
        [Route("api/admin/[controller]")]
        [Authorize(Roles = "Admin")]
        public class SessionApiController : ControllerBase
        {
            private readonly ISessionService _sessionService;

            public SessionApiController(
                ISessionService sessionService)
            {
                _sessionService = sessionService;
            }

            // GET: api/admin/SessionApi?pageNumber=1&pageSize=10
            [HttpGet]
            public IActionResult GetAll(
                int pageNumber = 1,
                int pageSize = 10)
            {
                var result =
                    _sessionService.GetAll(
                        pageNumber,
                        pageSize);

                return Ok(result);
            }

            // GET: api/admin/SessionApi/all
            [HttpGet("all")]
            public IActionResult GetAll()
            {
                var result =
                    _sessionService.GetAll();

                return Ok(result);
            }

            // GET: api/admin/SessionApi/1
            [HttpGet("{sessionId}")]
            public async Task<IActionResult> GetById(
                int sessionId)
            {
                var result =
                    await _sessionService.GetById(sessionId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Không tìm thấy Session"
                    });
                }

                return Ok(result);
            }

            // POST: api/admin/SessionApi
            [HttpPost]
            public async Task<IActionResult> Add(
                [FromBody] CreateSessionViewModel vm)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                try
                {
                    await _sessionService.Add(vm);

                    return Ok(new
                    {
                        message = "Thêm Session thành công"
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
using TrakingCar.Models;
using TrakingCar.Dto.ownership;
using TrakingCar.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TrakingCar.Data;
using TrakingCar.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tracking.Dto;
using TrackingCar.Dto;
using Tracking.Middlewares;

namespace TrakingCar.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class OwnershipController : ControllerBase
    {
        private readonly IOwnershipRepository _ownershipRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OwnershipController> _logger;
        private readonly IAuditLogger _auditLogger;
        private APIResponse _response;

        public OwnershipController(IOwnershipRepository ownershipRepository, ApplicationDbContext context, ILogger<OwnershipController> logger,
 IAuditLogger auditLogger) 
        {
            _ownershipRepository = ownershipRepository;
            _context = context;
            _logger = logger;
            _auditLogger = auditLogger;
            _response = new APIResponse();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetAllOwnerships(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? search = null)
        {
            try
            {
                var total = await _ownershipRepository.GetCountAsync(search);
                var paged = await _ownershipRepository.GetPaginatedAsync(pageNumber, pageSize, search);

                var result = new PaginatedResponse<Ownership>
                {
                    Data = paged,
                    TotalRecords = total,
                    Page = pageNumber,
                    PageSize = pageSize
                };

                _response.Result = result;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, _response);
            }
        }

        [HttpGet("{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetOwnership(Guid id)
        {
            try
            {
                var ownership = await _ownershipRepository.GetAsync(o => o.Id == id);
                if (ownership == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = ownership;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(500, _response);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<APIResponse>> CreateOwnership([FromBody] CreateOwnershipDto dto)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                {
                    await _auditLogger.LogAsync(
                        "اضافة",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل الإدخال: البيانات غير صالحة",
                        400,
                        userName,
                        ip);

                    return BadRequest("البيانات المدخلة غير صالحة.");
                }

                var created = await _ownershipRepository.CreateOwnershipAsync(dto);
                if (!created)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "فشل إنشاء الملكية." };

                    await _auditLogger.LogAsync(
                        "اضافة",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل العملية: لم يتم إنشاء الملكية",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;

                await _auditLogger.LogAsync(
                    "اضافة",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                    $"✅ تم إنشاء الملكية بنجاح: '{dto.Name}'",
                    201,
                    userName,
                    ip);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };

                await _auditLogger.LogAsync(
                    "اضافة",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode(500, _response);
            }
        }

        [HttpPut("{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> UpdateOwnership(Guid id, [FromBody] UpdateOwnershipDto dto)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                var existing = await _ownershipRepository.GetAsync(o => o.Id == id);
                if (existing == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Ownership not found." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل العملية: الملكية غير موجودة",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                var updated = await _ownershipRepository.UpdateOwnershipAsync(id, dto);
                if (!updated)
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "فشل تحديث الملكية." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل العملية: لم يتم تحديث الملكية",
                        500,
                        userName,
                        ip);

                    return StatusCode((int)HttpStatusCode.InternalServerError, _response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = new List<string> { "تم التعديل بنجاح" };

                await _auditLogger.LogAsync(
                    "تعديل",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                    $"✅ تم تعديل الملكية بنجاح: {id}",
                    200,
                    userName,
                    ip);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };

                await _auditLogger.LogAsync(
                    "تعديل",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

        [HttpDelete("{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> DeleteOwnership(Guid id)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            try
            {
                var ownership = await _ownershipRepository.GetAsync(o => o.Id == id);
                if (ownership == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Ownership not found." };

                    await _auditLogger.LogAsync(
                        "حذف",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: OwnershipId = {id}",
                        "❌ فشل العملية: الملكية غير موجودة",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                bool isUsed = await _context.Cars.AnyAsync(c => c.OwnershipId == id);
                if (isUsed)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "لا يمكن حذف الملكية لأنها مرتبطة بسيارات." };

                    await _auditLogger.LogAsync(
                        "حذف",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: OwnershipId = {id}",
                        "❌ فشل العملية: الملكية مرتبطة بسيارات",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                await _ownershipRepository.RemoveAsync(ownership);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = new { Message = "تم حذف الملكية بنجاح.", OwnershipId = id };

                await _auditLogger.LogAsync(
                    "حذف",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: OwnershipId = {id}",
                    $"✅ تم حذف الملكية بنجاح: {id}",
                    200,
                    userName,
                    ip);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };

                await _auditLogger.LogAsync(
                    "حذف",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: OwnershipId = {id}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }


        [HttpGet("details/{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetOwnershipDetails(Guid id)
        {
            try
            {
                var ownershipDetails = await _ownershipRepository.GetOwnershipDetailsAsync(id);
                if (ownershipDetails == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "الملكية غير موجودة.";
                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = ownershipDetails;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
                return StatusCode(500, _response);
            }
        }
    }
}

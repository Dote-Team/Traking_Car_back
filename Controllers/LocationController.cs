using TrakingCar.Models;
using TrakingCar.Dto.location;
using TrakingCar.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TrakingCar.Data;
using TrakingCar.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tracking.Dto;
using TrackingCar.Dto;
using TrackingCar.Middlewares;

namespace TrakingCar.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LocationController> _logger;
        private readonly IAuditLogger _auditLogger;
        private APIResponse _response;

        public LocationController(ILocationRepository locationRepository, ApplicationDbContext context, ILogger<LocationController> logger,
        IAuditLogger auditLogger) 
        {
            _locationRepository = locationRepository;
            _context = context;
            _logger = logger;
            _auditLogger = auditLogger;
            _response = new APIResponse();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetAllLocations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? search = null)
        {
            try
            {
                var total = await _locationRepository.GetCountAsync(search);
                var paged = await _locationRepository.GetPaginatedAsync(pageNumber, pageSize, search);

                var result = new PaginatedResponse<Location>
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
        public async Task<ActionResult<APIResponse>> GetLocation(Guid id)
        {
            try
            {
                var location = await _locationRepository.GetAsync(l => l.Id == id);
                if (location == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = location;
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
        public async Task<ActionResult<APIResponse>> CreateLocation([FromBody] CreateLocationDto dto)
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

                var existing = await _context.Locations
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Name.ToLower() == dto.Name.ToLower() && l.DeletedAt == null);

                if (existing != null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "اسم الموقع مستخدم مسبقًا. يرجى اختيار اسم مختلف." };

                    await _auditLogger.LogAsync(
                        "اضافة",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        $"❌ فشل العملية: اسم الموقع '{dto.Name}' موجود مسبقًا",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                var created = await _locationRepository.CreateLocationAsync(dto);
                if (!created)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "فشل إنشاء الموقع." };

                    await _auditLogger.LogAsync(
                        "اضافة",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        $"❌ فشل العملية: لم يتم إنشاء الموقع '{dto.Name}'",
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
                    $"✅ تم إنشاء الموقع بنجاح: '{dto.Name}'",
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
        public async Task<ActionResult<APIResponse>> UpdateLocation(Guid id, [FromBody] UpdateLocationDto dto)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                var existing = await _locationRepository.GetAsync(l => l.Id == id);
                if (existing == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Location not found." };
                    _response.Message = "لم يتم العثور على الموقع المطلوب للتعديل.";

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل العملية: الموقع غير موجود",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                var duplicate = await _context.Locations
                    .IgnoreQueryFilters()
                    .AnyAsync(l => l.Name.ToLower() == dto.Name.ToLower() && l.Id != id && l.DeletedAt == null);

                if (duplicate)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "اسم الموقع مستخدم مسبقًا." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        $"❌ فشل العملية: اسم الموقع '{dto.Name}' مستخدم مسبقًا",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                var updated = await _locationRepository.UpdateLocationAsync(id, dto);
                if (!updated)
                {
                    _response.StatusCode = HttpStatusCode.InternalServerError;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "فشل تحديث الموقع." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dto, jsonOptions)}",
                        "❌ فشل العملية: لم يتم تحديث الموقع",
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
                    $"✅ تم تعديل الموقع بنجاح: {id}",
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
                _response.Message = "حدث خطأ أثناء تنفيذ الطلب.";

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
        public async Task<ActionResult<APIResponse>> DeleteLocation(Guid id)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            try
            {
                var location = await _locationRepository.GetAsync(l => l.Id == id);
                if (location == null)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Location not found." };

                    await _auditLogger.LogAsync(
                        "حذف",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: LocationId = {id}",
                        "❌ فشل العملية: الموقع غير موجود",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                bool isUsed = await _context.Cars.AnyAsync(c => c.LocationId == id);
                if (isUsed)
                {
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "لا يمكن حذف الموقع لأنه مرتبط بسيارات." };

                    await _auditLogger.LogAsync(
                        "حذف",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: LocationId = {id}",
                        "❌ فشل العملية: الموقع مرتبط بسيارات",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                await _locationRepository.RemoveAsync(location);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = new
                {
                    Message = "تم حذف الموقع بنجاح.",
                    LocationId = id
                };

                await _auditLogger.LogAsync(
                    "حذف",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: LocationId = {id}",
                    $"✅ تم حذف الموقع بنجاح: {id}",
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
                    $"البيانات المدخلة: LocationId = {id}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }


        [HttpGet("details/{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetLocationDetails(Guid id)
        {
            try
            {
                var locationDetails = await _locationRepository.GetLocationDetailsAsync(id);
                if (locationDetails == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.Message = "الموقع غير موجود.";
                    return NotFound(_response);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = locationDetails;
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

using TrakingCar.Models;
using TrakingCar.Dto.Car;
using TrakingCar.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TrakingCar.Data;
using TrakingCar.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Tracking.Dto;
using TrackingCar.Dto;
using TrakingCar.Dto.car;
using Tracking.Middlewares;

namespace TrakingCar.Controllers
{
    [Route("API/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly ICarRepository _carRepository;
        private readonly ApplicationDbContext _context;
        private APIResponse _response;
        private readonly ILogger<CarController> _logger;
        private readonly IAuditLogger _auditLogger;

        public CarController(ICarRepository carRepository, ApplicationDbContext context, ILogger<CarController> logger,
            IAuditLogger auditLogger) 
        {
            _carRepository = carRepository;
            _context = context;
            _logger = logger;
            _auditLogger = auditLogger;
            _response = new APIResponse();
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<APIResponse>> GetAllCars(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? search = null)
        {
            try
            {
                var total = await _carRepository.GetCountAsync(search);
                var paged = await _carRepository.GetPaginatedAsync(pageNumber, pageSize, search);

                var result = new PaginatedResponse<Car>
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
        public async Task<ActionResult<APIResponse>> GetCarDetails(Guid id)
        {
            try
            {
                var car = await _carRepository.GetCarDetailsAsync(id);
                if (car == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = car;
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

        [HttpPost("create")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> CreateCars([FromForm] List<CreateCarDto> dtos)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                if (dtos == null || !dtos.Any())
                {
                    await _auditLogger.LogAsync(
                        actionType: "اضافة",
                        path: HttpContext.Request.Path,
                        requestData: $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dtos, jsonOptions)}",
                        responseData: "❌ فشل الإدخال: البيانات غير صالحة",
                        statusCode: 400,
                        userName: userName,
                        ip: ip);

                    return BadRequest("البيانات المدخلة غير صالحة.");
                }

                var created = await _carRepository.CreateCarsAsync(dtos);

                if (!created)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages = new List<string> { "فشل إنشاء السيارات." };

                    await _auditLogger.LogAsync(
                        "اضافة",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dtos, jsonOptions)}",
                        $"❌ فشل العملية: لم يتم إنشاء السيارات",
                        400,
                        userName,
                        ip);

                    return BadRequest(_response);
                }

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                _response.Result = new { Message = "تم إنشاء السيارات بنجاح" };

                await _auditLogger.LogAsync(
                    "اضافة",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dtos, jsonOptions)}",
                    $"✅ تم إنشاء السيارات بنجاح",
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
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(dtos, jsonOptions)}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode(500, _response);
            }
        }



        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<APIResponse>> UpdateCar(Guid id, [FromForm] UpdateCarDto carUpdateDto)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            var jsonOptions = new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            try
            {
                // تحقق من وجود السيارة
                var existingCar = await _carRepository.GetCarDetailsAsync(id);
                if (existingCar == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Car not found." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                        "❌ فشل العملية: السيارة غير موجودة",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                // تحقق من تكرار CarNumber
                if (!string.IsNullOrWhiteSpace(carUpdateDto.CarNumber))
                {
                    var duplicate = await _context.Cars
                        .IgnoreQueryFilters()
                        .AnyAsync(c => c.CarNumber.ToLower() == carUpdateDto.CarNumber.ToLower() && c.Id != id && c.DeletedAt == null);

                    if (duplicate)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        _response.ErrorMessages = new List<string> { "رقم السيارة مستخدم مسبقًا." };

                        await _auditLogger.LogAsync(
                            "تعديل",
                            HttpContext.Request.Path,
                            $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                            $"❌ فشل العملية: رقم السيارة '{carUpdateDto.CarNumber}' مستخدم مسبقًا",
                            400,
                            userName,
                            ip);

                        return BadRequest(_response);
                    }
                }

                var result = await _carRepository.UpdateCarAsync(id, carUpdateDto);
                if (result)
                {
                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    _response.Result = new { Message = "تم تعديل السيارة بنجاح." };

                    await _auditLogger.LogAsync(
                        "تعديل",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                        $"✅ تم تعديل السيارة بنجاح: {id}",
                        200,
                        userName,
                        ip);

                    return Ok(_response);
                }

                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "لم يتم العثور على السيارة أو حدث خطأ أثناء التعديل." };

                await _auditLogger.LogAsync(
                    "تعديل",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                    "❌ فشل العملية: لم يتم العثور على السيارة أو حدث خطأ أثناء التعديل",
                    404,
                    userName,
                    ip);

                return NotFound(_response);
            }
            catch (DbUpdateException dbEx)
            {
                var sqlMessage = dbEx.InnerException?.Message;
                _response.StatusCode = HttpStatusCode.Conflict;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { "حدث خطأ أثناء حفظ البيانات.", sqlMessage };

                await _auditLogger.LogAsync(
                    "تعديل",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                    $"❌ خطأ قاعدة بيانات: {sqlMessage}",
                    409,
                    userName,
                    ip);

                return Conflict(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message, ex.InnerException?.Message };

                await _auditLogger.LogAsync(
                    "تعديل",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: {System.Text.Json.JsonSerializer.Serialize(carUpdateDto, jsonOptions)}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }



        [HttpDelete("{id:Guid}")]
        [Authorize]
        public async Task<ActionResult<APIResponse>> DeleteCar(Guid id)
        {
            var userName = User.Identity?.Name ?? "مجهول";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "غير معروف";

            try
            {
                var car = await _carRepository.GetCarDetailsAsync(id);
                if (car == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "Car not found." };

                    await _auditLogger.LogAsync(
                        "حذف",
                        HttpContext.Request.Path,
                        $"البيانات المدخلة: CarId = {id}",
                        "❌ فشل العملية: السيارة غير موجودة",
                        404,
                        userName,
                        ip);

                    return NotFound(_response);
                }

                await _carRepository.RemoveAsync(car);

                _response.StatusCode = HttpStatusCode.OK;
                _response.IsSuccess = true;
                _response.Result = new
                {
                    Message = "تم حذف السيارة بنجاح.",
                    CarId = id
                };

                await _auditLogger.LogAsync(
                    "حذف",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: CarId = {id}",
                    $"✅ تم حذف السيارة بنجاح: {id}",
                    200,
                    userName,
                    ip);

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message, ex.InnerException?.Message };

                await _auditLogger.LogAsync(
                    "حذف",
                    HttpContext.Request.Path,
                    $"البيانات المدخلة: CarId = {id}",
                    $"❌ خطأ استثنائي أثناء العملية: {ex.Message}",
                    500,
                    userName,
                    ip);

                return StatusCode((int)HttpStatusCode.InternalServerError, _response);
            }
        }

    }
}

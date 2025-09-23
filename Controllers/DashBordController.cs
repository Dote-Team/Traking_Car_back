using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using TrakingCar.Data;
using TrackingCar.Dto;
using TrakingCar.Dto.dashbord;

namespace TrackingCar.Controllers
{
    [Route("API/[controller]")]
    [ApiController]

    public class DashBordController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private APIResponse _response;

        public DashBordController(ApplicationDbContext context)
        {
            _context = context;
            _response = new APIResponse();
        }




        [Authorize]
        [HttpGet("cars-by-date")]
        public async Task<ActionResult<APIResponse>> GetCarsByDate(
            int? year = null,
            int? month = null,
            string groupBy = "month"
        )
        {
            try
            {
                var cars = await _context.Cars
                    .Where(c => c.DeletedAt == null)
                    .ToListAsync();

                cars = cars
                    .Where(c =>
                        (!year.HasValue || c.CreatedAt.Value.Year == year.Value) &&
                        (!month.HasValue || c.CreatedAt.Value.Month == month.Value))
                    .ToList();

                List<CarsByDateDto> result = new();

                switch (groupBy.ToLower())
                {
                    case "week":
                        var dayGroups = cars
                            .GroupBy(c => c.CreatedAt.Value.DayOfWeek)
                            .ToDictionary(g => g.Key, g => g.Count());

                        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                        {
                            result.Add(new CarsByDateDto
                            {
                                Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(day),
                                TotalCars = dayGroups.ContainsKey(day) ? dayGroups[day] : 0
                            });
                        }
                        break;

                    case "year":
                        var yearGroups = cars
                            .GroupBy(c => c.CreatedAt.Value.Year)
                            .ToDictionary(g => g.Key, g => g.Count());

                        int baseYear = year ?? DateTime.UtcNow.Year;
                        for (int i = baseYear - 2; i <= baseYear; i++)
                        {
                            result.Add(new CarsByDateDto
                            {
                                Date = i.ToString(),
                                TotalCars = yearGroups.ContainsKey(i) ? yearGroups[i] : 0
                            });
                        }
                        break;

                    case "month":
                    default:
                        var monthGroups = cars
                            .GroupBy(c => c.CreatedAt.Value.Month)
                            .ToDictionary(g => g.Key, g => g.Count());

                        for (int i = 1; i <= 12; i++)
                        {
                            result.Add(new CarsByDateDto
                            {
                                Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(i),
                                TotalCars = monthGroups.ContainsKey(i) ? monthGroups[i] : 0
                            });
                        }
                        break;
                }

                _response.Result = result;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return Ok(_response);
        }

        [Authorize]
        [HttpGet("cars-max-date")]
        public async Task<ActionResult<APIResponse>> GetCarsMaxDate(string groupBy = "month")
        {
            try
            {
                var cars = await _context.Cars
                    .Where(c => c.DeletedAt == null)
                    .ToListAsync();

                CarsByDateDto maxResult = null;

                switch (groupBy.ToLower())
                {
                    case "week":
                        maxResult = cars
                            .GroupBy(c => c.CreatedAt.Value.DayOfWeek)
                            .Select(g => new CarsByDateDto
                            {
                                Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(g.Key),
                                TotalCars = g.Count()
                            })
                            .OrderByDescending(x => x.TotalCars)
                            .FirstOrDefault();
                        break;

                    case "year":
                        maxResult = cars
                            .GroupBy(c => c.CreatedAt.Value.Year)
                            .Select(g => new CarsByDateDto
                            {
                                Date = g.Key.ToString(),
                                TotalCars = g.Count()
                            })
                            .OrderByDescending(x => x.TotalCars)
                            .FirstOrDefault();
                        break;

                    case "month":
                    default:
                        maxResult = cars
                            .GroupBy(c => c.CreatedAt.Value.Month)
                            .Select(g => new CarsByDateDto
                            {
                                Date = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key),
                                TotalCars = g.Count()
                            })
                            .OrderByDescending(x => x.TotalCars)
                            .FirstOrDefault();
                        break;
                }

                _response.Result = maxResult ?? new CarsByDateDto { Date = "N/A", TotalCars = 0 };
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return Ok(_response);
        }


        [Authorize]
        [HttpGet("top-locations")]
        public async Task<ActionResult<APIResponse>> GetTopLocations()
        {
            try
            {
                var topLocations = await _context.Cars
                    .Where(c => c.DeletedAt == null && c.Location != null)
                    .GroupBy(c => c.Location.Name) // اسم الموقع
                    .Select(g => new CarsByGroupDto
                    {
                        Name = g.Key,
                        TotalCars = g.Count()
                    })
                    .OrderByDescending(x => x.TotalCars)
                    .Take(10)
                    .ToListAsync();

                _response.Result = topLocations;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return Ok(_response);
        }



        [Authorize]
        [HttpGet("top-ownerships")]
        public async Task<ActionResult<APIResponse>> GetTopOwnerships()
        {
            try
            {
                var topOwnerships = await _context.Cars
                    .Where(c => c.DeletedAt == null && c.Ownership != null)
                    .GroupBy(c => c.Ownership.Name) // اسم المالك
                    .Select(g => new CarsByGroupDto
                    {
                        Name = g.Key,
                        TotalCars = g.Count()
                    })
                    .OrderByDescending(x => x.TotalCars)
                    .Take(10)
                    .ToListAsync();

                _response.Result = topOwnerships;
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.OK;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                _response.StatusCode = HttpStatusCode.InternalServerError;
            }

            return Ok(_response);
        }

    }




}





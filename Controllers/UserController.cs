
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Tracking.Dto;
using Microsoft.EntityFrameworkCore;
using TrackingCar.Interfaces;
using TrackingCar.Dto.user;
using TrackingCar.Dto;

namespace Weapon.Controllers
{
    [Route("API/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepo;
        protected APIResponse _response;

        public UserController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _response = new APIResponse();
        }



        [HttpPost("registeruser")]
        [Consumes("multipart/form-data")] // ✅ استقبال بيانات مع ملفات
        public async Task<ActionResult<APIResponse>> RegisterUser([FromForm] RegisterationRequestDto registerationRequestDto)
        {
            try
            {
                var user = await _userRepo.RegisterUser(registerationRequestDto);
                return Ok(new APIResponse
                {
                    StatusCode = HttpStatusCode.Created,
                    IsSuccess = true,
                    Result = user
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new APIResponse
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    ErrorMessages = new List<string> { ex.Message }
                });
            }
        }



        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<APIResponse>> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            try
            {
                if (loginRequestDto == null)
                    return BadRequest(loginRequestDto);

                var loginResponseDto = await _userRepo.Login(loginRequestDto);
                if (loginResponseDto == null)
                    return Unauthorized();

                // _response.Result = loginResponseDto;
                // _response.StatusCode = HttpStatusCode.OK;

                return Ok(loginResponseDto);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [Authorize]
        [HttpGet("{id}", Name = "GetUserById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userRepo.GetAsync(u => u.Id == id);
                if (user == null)
                    return NotFound();

                _response.Result = user;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<APIResponse>> GetUsers()
        {
            try
            {
                var users = await _userRepo.GetAllAsync();
                if (users == null)
                    return NotFound();

                _response.Result = users;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _userRepo.GetAsync(u => u.Id == id);
                if (user == null)
                    return NotFound(new { message = "User not found." });

                await _userRepo.RemoveAsync(user);
                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while deleting the user: {ex.Message}" });
            }
        }



        [HttpPut("{id}")]
        [Authorize]
        [Consumes("multipart/form-data")] // ✅ لدعم رفع الملفات
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromForm] UserUpdateDto userUpdateDto)
        {
            try
            {
                if (userUpdateDto == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Invalid data provided."
                    });
                }

                var result = await _userRepo.UpdateUserAsync(id, userUpdateDto);

                if (!result)
                {
                    return NotFound(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Message = "User not found."
                    });
                }

                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Message = "User updated successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "An error occurred while updating the user.",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenRequest)
        {
            if (refreshTokenRequest == null || string.IsNullOrEmpty(refreshTokenRequest.RefreshToken))
                return BadRequest("Invalid client request");

            var response = await _userRepo.RefreshTokenAsync(refreshTokenRequest.RefreshToken);
            if (response == null)
                return Unauthorized("Invalid or expired refresh token");

            return Ok(response);
        }

        [Authorize]
        [HttpGet("all-users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<APIResponse>> GetAllUsers(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var paginatedUsers = await _userRepo.GetAllUsersAsync(search, page, pageSize);

                // ✅ Ensure pagination data is included
                _response.Result = new
                {
                    Data = paginatedUsers.Data,
                    Pagination = paginatedUsers.Metadata
                };

                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.InternalServerError;
                _response.ErrorMessages = new List<string> { $"❌ Internal Server Error: {ex.Message}" };
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }




        [HttpGet("removed", Name = "GetRemovedUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetRemovedUsers()
        {
            try
            {
                var removedUsers = await _userRepo.GetRemovedAsync();
                _response.Result = removedUsers;
                _response.StatusCode = HttpStatusCode.OK;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string> { ex.Message };
                return StatusCode(StatusCodes.Status500InternalServerError, _response);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize] 
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Message = "Invalid data provided."
                    });
                }

                var result = await _userRepo.UpdateUserStatusAsync(id, dto.Status);
                if (!result)
                {
                    return NotFound(new
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        IsSuccess = false,
                        Message = "User not found."
                    });
                }

                return Ok(new
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true,
                    Message = "User status updated successfully.",
                    UserId = id,
                    NewStatus = dto.Status
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user status: {ex.Message}");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                    Message = "An error occurred while updating user status.",
                    Error = ex.Message
                });
            }
        }



    }
}





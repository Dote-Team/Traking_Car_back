using Tracking.Dto;
using TrackingCar.Dto;
using TrackingCar.Dto.user;
using TrackingCar.Interfaces;
using TrakingCar.Data;



namespace TrackingCar.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<bool> IsPasswordCorrect(RegisterationRequestDto registrationRequestDto);
        Task<bool> IsUniqueUser(string userName);
        Task<UserDto> RegisterUser(RegisterationRequestDto registrationRequestDto);
        Task<bool> UserExistsAsync(Guid id);
        Task<bool> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task<APIResponse> Login(LoginRequestDto loginRequestDto);
        Task<IEnumerable<User>> GetRemovedAsync();
        Task<PaginatedResponse<UserDto>> GetAllUsersAsync(string? search, int page, int pageSize);

        Task<bool> UpdateUserStatusAsync(Guid id, bool status);


    }
}

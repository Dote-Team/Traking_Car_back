using TrakingCar.Models;
using TrakingCar.Dto.location;
using TrackingCar.Interfaces;

namespace TrakingCar.Interfaces
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<bool> CreateLocationAsync(CreateLocationDto dto);
        Task<bool> UpdateLocationAsync(Guid id, UpdateLocationDto dto);

        Task<IEnumerable<Location>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
        Task<int> GetCountAsync(string? search = null);
        Task<LocationDetailsDto?> GetLocationDetailsAsync(Guid id);

    }
}

using TrakingCar.Models;
using TrakingCar.Dto.ownership;
using TrackingCar.Interfaces;

namespace TrakingCar.Interfaces
{
    public interface IOwnershipRepository : IRepository<Ownership>
    {
        Task<bool> CreateOwnershipAsync(CreateOwnershipDto dto);
        Task<bool> UpdateOwnershipAsync(Guid id, UpdateOwnershipDto dto);
        Task<IEnumerable<Ownership>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
        Task<int> GetCountAsync(string? search = null);
        Task<OwnershipDetailsDto?> GetOwnershipDetailsAsync(Guid id);
    }
}

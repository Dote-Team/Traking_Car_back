using TrakingCar.Models;
using TrakingCar.Dto.Car;
using TrackingCar.Interfaces;
using TrakingCar.Dtos;

namespace TrakingCar.Interfaces
{
    public interface ICarRepository : IRepository<Car>
    {
        Task<bool> CreateCarsAsync(List<CreateCarDto> dtos);
        Task<bool> UpdateCarAsync(Guid id, UpdateCarDto updateDto);

        Task<IEnumerable<CarDto>> GetPaginatedAsync(
                    int pageNumber,
                    int pageSize,
                    string? search = null,
                    Guid? locationId = null,
                    Guid? ownershipId = null);
        Task<int> GetCountAsync(string? search = null, Guid? locationId = null, Guid? ownershipId = null);

        Task<Car?> GetCarDetailsAsync(Guid id);
    }
}

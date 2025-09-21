using TrakingCar.Models;
using TrakingCar.Dto.Car;
using TrackingCar.Interfaces;
using TrakingCar.Dto.car;

namespace TrakingCar.Interfaces
{
    public interface ICarRepository : IRepository<Car>
    {
        Task<bool> CreateCarsAsync(List<CreateCarDto> dtos);
        Task<bool> UpdateCarAsync(Guid id, UpdateCarDto updateDto);

        Task<IEnumerable<Car>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null);
        Task<int> GetCountAsync(string? search = null);

        Task<Car?> GetCarDetailsAsync(Guid id);
    }
}

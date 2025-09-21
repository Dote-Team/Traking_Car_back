using TrakingCar.Models;
using TrakingCar.Dto.location;
using TrakingCar.Interfaces;
using TrakingCar.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TrackingCar.Repositories;
using TrakingCar.Dtos;

namespace TrakingCar.Repositories
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public LocationRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CreateLocationAsync(CreateLocationDto dto)
        {
            var location = _mapper.Map<Location>(dto);
            await CreateAsync(location);
            return true;
        }

        public async Task<bool> UpdateLocationAsync(Guid id, UpdateLocationDto dto)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return false;

            _mapper.Map(dto, location);
            await UpdateAsync(location);
            return true;
        }

        public async Task<IEnumerable<Location>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            var query = _context.Locations.Where(l => l.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.Name != null && l.Name.Contains(search));

            return await query
                .OrderBy(l => l.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? search = null)
        {
            var query = _context.Locations.Where(l => l.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.Name != null && l.Name.Contains(search));

            return await query.CountAsync();
        }

        public async Task<LocationDetailsDto?> GetLocationDetailsAsync(Guid id)
        {
            var location = await _context.Locations
                .Include(l => l.Cars)           
                .Include(l => l.Attachments)    
                .FirstOrDefaultAsync(l => l.Id == id && l.DeletedAt == null);

            if (location == null)
                return null;

            var details = new LocationDetailsDto
            {
                Id = location.Id,
                Name = location.Name,
                Detailes = location.Detailes,
                LocationName = location.LocationName,
                Cars = location.Cars?.Select(c => new CarDto
                {
                    Id = c.Id,
                    CarNumber = c.CarNumber,
                    CarType = c.CarType,
                    ChassisNumber = c.ChassisNumber
                }).ToList(),
                Attachments = location.Attachments?.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    File = a.File

                }).ToList()
            };

            return details;
        }

    }
}

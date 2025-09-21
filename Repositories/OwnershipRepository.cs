using TrakingCar.Models;
using TrakingCar.Dto.ownership;
using TrakingCar.Interfaces;
using TrakingCar.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TrakingCar.Dtos;
using TrackingCar.Repositories;

namespace TrakingCar.Repositories
{
    public class OwnershipRepository : Repository<Ownership>, IOwnershipRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OwnershipRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<bool> CreateOwnershipAsync(CreateOwnershipDto dto)
        {
            var ownership = _mapper.Map<Ownership>(dto);
            await CreateAsync(ownership);
            return true;
        }

        public async Task<bool> UpdateOwnershipAsync(Guid id, UpdateOwnershipDto dto)
        {
            var ownership = await _context.Ownerships.FindAsync(id);
            if (ownership == null)
                return false;

            _mapper.Map(dto, ownership);
            await UpdateAsync(ownership);
            return true;
        }

        public async Task<IEnumerable<Ownership>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            var query = _context.Ownerships.Where(o => o.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Name != null && o.Name.Contains(search));

            return await query
                .OrderBy(o => o.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? search = null)
        {
            var query = _context.Ownerships.Where(o => o.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.Name != null && o.Name.Contains(search));

            return await query.CountAsync();
        }

        public async Task<OwnershipDetailsDto?> GetOwnershipDetailsAsync(Guid id)
        {
            var ownership = await _context.Ownerships
                .Include(o => o.Cars)
                .FirstOrDefaultAsync(o => o.Id == id && o.DeletedAt == null);

            if (ownership == null)
                return null;

            var details = new OwnershipDetailsDto
            {
                Id = ownership.Id,
                Name = ownership.Name,
                Detailes = ownership.Detailes,
                LocationName = ownership.LocationName,
                LocationId = ownership.LocationId,
                Cars = ownership.Cars?.Select(c => new CarDto
                {
                    Id = c.Id,
                    CarNumber = c.CarNumber,
                    CarType = c.CarType,
                    ChassisNumber = c.ChassisNumber
                }).ToList()
            };

            return details;
        }
    }
}

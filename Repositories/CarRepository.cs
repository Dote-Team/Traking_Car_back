using TrakingCar.Models;
using TrakingCar.Dto.Car;
using TrakingCar.Interfaces;
using TrakingCar.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TrackingCar.Repositories;
using TrakingCar.Dto.car;

namespace TrakingCar.Repositories
{
    public class CarRepository : Repository<Car>, ICarRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CarRepository(ApplicationDbContext context, IMapper mapper) : base(context)
        {
            _context = context;
            _mapper = mapper;
        }

        //public async Task<bool> CreateCarsAsync(List<CreateCarDto> dtos)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        foreach (var dto in dtos)
        //        {
        //            var car = new Car
        //            {
        //                CarType = dto.CarType,
        //                ChassisNumber = dto.ChassisNumber,
        //                CarNumber = dto.CarNumber,
        //                Status = dto.Status,
        //                OwnershipId = dto.OwnershipId,
        //                LocationId = dto.LocationId,
        //                ReceiptDate = dto.ReceiptDate,
        //                BodyCondition = dto.BodyCondition,
        //                Note = dto.Note,
        //                TrackingCode = dto.TrackingCode,
        //                CreatedAt = DateTime.UtcNow,
        //                UpdatedAt = DateTime.UtcNow
        //            };

        //            // حفظ المرفقات
        //            if (dto.Attachments != null && dto.Attachments.Any())
        //            {
        //                car.Attachments = new List<Attachment>();
        //                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "CarAttachments");
        //                if (!Directory.Exists(uploadPath))
        //                    Directory.CreateDirectory(uploadPath);

        //                foreach (var attach in dto.Attachments)
        //                {
        //                    if (attach.FileContent != null)
        //                    {
        //                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(attach.FileContent.FileName)}";
        //                        var filePath = Path.Combine(uploadPath, fileName);

        //                        using var stream = new FileStream(filePath, FileMode.Create);
        //                        await attach.FileContent.CopyToAsync(stream);

        //                        car.Attachments.Add(new Attachment
        //                        {
        //                            File = fileName,
        //                            LocationId = attach.LocationId
        //                        });
        //                    }
        //                }
        //            }

        //            await _context.Cars.AddAsync(car);
        //        }

        //        await _context.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        return true;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task<bool> CreateCarsAsync(List<CreateCarDto> dtos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "CarAttachments");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var dto in dtos)
                {
                    var car = new Car
                    {
                        CarType = dto.CarType,
                        ChassisNumber = dto.ChassisNumber,
                        CarNumber = dto.CarNumber,
                        Status = dto.Status,
                        OwnershipId = dto.OwnershipId,
                        LocationId = dto.LocationId,
                        ReceiptDate = dto.ReceiptDate,
                        BodyCondition = dto.BodyCondition,
                        Note = dto.Note,
                        TrackingCode = dto.TrackingCode,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    car.Attachments = new List<Attachment>();

                    // ✅ صورة سنوية
                    if (dto.AnnualImageFile != null)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.AnnualImageFile.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await dto.AnnualImageFile.CopyToAsync(stream);

                        car.Attachments.Add(new Attachment
                        {
                            File = fileName,
                            LocationId = dto.LocationId,
                            Type = "صورة سنوية",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    // ✅ صورة تخويل
                    if (dto.AuthorizationImageFile != null)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.AuthorizationImageFile.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await dto.AuthorizationImageFile.CopyToAsync(stream);

                        car.Attachments.Add(new Attachment
                        {
                            File = fileName,
                            LocationId = dto.LocationId,
                            Type = "صورة تخويل",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    // ✅ صورة مستند
                    if (dto.DocumentImageFile != null)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(dto.DocumentImageFile.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await dto.DocumentImageFile.CopyToAsync(stream);

                        car.Attachments.Add(new Attachment
                        {
                            File = fileName,
                            LocationId = dto.LocationId,
                            Type = "صورة مستند",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    await _context.Cars.AddAsync(car);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateCarAsync(Guid id, UpdateCarDto updateDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var car = await _context.Cars.Include(c => c.Attachments).FirstOrDefaultAsync(c => c.Id == id);
                if (car == null)
                    return false;

                // تحديث بيانات السيارة
                car.CarType = updateDto.CarType;
                car.ChassisNumber = updateDto.ChassisNumber;
                car.CarNumber = updateDto.CarNumber;
                car.Status = updateDto.Status;
                car.OwnershipId = updateDto.OwnershipId;
                car.LocationId = updateDto.LocationId;
                car.ReceiptDate = updateDto.ReceiptDate;
                car.BodyCondition = updateDto.BodyCondition;
                car.Note = updateDto.Note;
                car.TrackingCode = updateDto.TrackingCode;
                car.UpdatedAt = DateTime.UtcNow;

                // إدارة الملفات
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "CarAttachments");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // حذف الملفات القديمة
                if (car.Attachments != null && car.Attachments.Any())
                {
                    foreach (var attachment in car.Attachments)
                    {
                        var filePath = Path.Combine(uploadPath, attachment.File);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }

                    _context.Attachments.RemoveRange(car.Attachments);
                }

                // رفع الملفات الجديدة
                if (updateDto.AttachmentsFiles != null && updateDto.AttachmentsFiles.Any())
                {
                    car.Attachments = new List<Attachment>();
                    foreach (var file in updateDto.AttachmentsFiles)
                    {
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using var stream = new FileStream(filePath, FileMode.Create);
                        await file.CopyToAsync(stream);

                        car.Attachments.Add(new Attachment
                        {
                            File = fileName,
                            LocationId = updateDto.LocationId,
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<Car>> GetPaginatedAsync(int pageNumber, int pageSize, string? search = null)
        {
            var query = _context.Cars
                .Include(c => c.Attachments)
                .Where(c => c.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CarNumber.Contains(search) || c.CarType.Contains(search));
            }

            return await query
                .OrderBy(c => c.CarNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync(string? search = null)
        {
            var query = _context.Cars.Where(c => c.DeletedAt == null);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.CarNumber.Contains(search) || c.CarType.Contains(search));

            return await query.CountAsync();
        }

        public async Task<Car?> GetCarDetailsAsync(Guid id)
        {
            return await _context.Cars
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);
        }
    }
}

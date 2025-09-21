using TrakingCar.Models;
using TrakingCar.Dto.Car;
using TrakingCar.Interfaces;
using TrakingCar.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TrackingCar.Repositories;
using TrakingCar.Dtos;

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

                // مسار رفع الملفات
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "CarAttachments");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // رفع الملفات الجديدة حسب النوع مع حذف القديم فقط عند التغيير
                if (updateDto.AnnualImageFile != null)
                {
                    // حذف الصورة السنوية القديمة إذا كانت موجودة
                    var oldAnnual = car.Attachments?.FirstOrDefault(a => a.Type == "صورة سنوية");
                    if (oldAnnual != null)
                    {
                        var oldPath = Path.Combine(uploadPath, oldAnnual.File);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                        _context.Attachments.Remove(oldAnnual);
                    }

                    // رفع الملف الجديد
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(updateDto.AnnualImageFile.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await updateDto.AnnualImageFile.CopyToAsync(stream);

                    car.Attachments.Add(new Attachment
                    {
                        File = fileName,
                        LocationId = updateDto.LocationId,
                        Type = "صورة سنوية",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                if (updateDto.AuthorizationImageFile != null)
                {
                    var oldAuth = car.Attachments?.FirstOrDefault(a => a.Type == "صورة تخويل");
                    if (oldAuth != null)
                    {
                        var oldPath = Path.Combine(uploadPath, oldAuth.File);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                        _context.Attachments.Remove(oldAuth);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(updateDto.AuthorizationImageFile.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await updateDto.AuthorizationImageFile.CopyToAsync(stream);

                    car.Attachments.Add(new Attachment
                    {
                        File = fileName,
                        LocationId = updateDto.LocationId,
                        Type = "صورة تخويل",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                if (updateDto.DocumentImageFile != null)
                {
                    var oldDoc = car.Attachments?.FirstOrDefault(a => a.Type == "صورة مستند");
                    if (oldDoc != null)
                    {
                        var oldPath = Path.Combine(uploadPath, oldDoc.File);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                        _context.Attachments.Remove(oldDoc);
                    }

                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(updateDto.DocumentImageFile.FileName)}";
                    var filePath = Path.Combine(uploadPath, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await updateDto.DocumentImageFile.CopyToAsync(stream);

                    car.Attachments.Add(new Attachment
                    {
                        File = fileName,
                        LocationId = updateDto.LocationId,
                        Type = "صورة مستند",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
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

        public async Task<IEnumerable<CarDto>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            Guid? locationId = null,
            Guid? ownershipId = null)
        {
            var query = _context.Cars
                .Include(c => c.Attachments)
                .Include(c => c.Location)
                .Include(c => c.Ownership)
                .Where(c => c.DeletedAt == null);

            // ✅ فلترة بالكلمة المفتاحية
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    (c.CarNumber != null && c.CarNumber.Contains(search)) ||
                    (c.CarType != null && c.CarType.Contains(search)) ||
                    (c.ChassisNumber != null && c.ChassisNumber.Contains(search)));
            }

            // ✅ فلترة بالـ LocationId
            if (locationId.HasValue)
            {
                query = query.Where(c => c.LocationId == locationId.Value);
            }

            // ✅ فلترة بالـ OwnershipId
            if (ownershipId.HasValue)
            {
                query = query.Where(c => c.OwnershipId == ownershipId.Value);
            }

            return await query
                .OrderBy(c => c.CarNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    CarType = c.CarType,
                    CarNumber = c.CarNumber,
                    ChassisNumber = c.ChassisNumber,
                    Status = c.Status,
                    TrackingCode = c.TrackingCode,
                    BodyCondition = c.BodyCondition,
                    Note = c.Note,
                    ReceiptDate = c.ReceiptDate,
                    LocationId = c.LocationId,
                    OwnershipId = c.OwnershipId,
                    LocationName = c.Location != null ? c.Location.Name : null,
                    OwnershipName = c.Ownership != null ? c.Ownership.Name : null,
                    Attachments = c.Attachments.Select(a => new AttachmentDto
                    {
                        Id = a.Id,
                        File = a.File,
                        Type = a.Type
                    }).ToList()
                })
                .ToListAsync();
        }


        public async Task<int> GetCountAsync(string? search = null, Guid? locationId = null, Guid? ownershipId = null)
        {
            var query = _context.Cars
                .Where(c => c.DeletedAt == null);

            // فلترة بالكلمة المفتاحية
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    (c.CarNumber != null && c.CarNumber.Contains(search)) ||
                    (c.CarType != null && c.CarType.Contains(search)) ||
                    (c.ChassisNumber != null && c.ChassisNumber.Contains(search)));
            }

            // فلترة بالـ LocationId
            if (locationId.HasValue)
            {
                query = query.Where(c => c.LocationId == locationId.Value);
            }

            // فلترة بالـ OwnershipId
            if (ownershipId.HasValue)
            {
                query = query.Where(c => c.OwnershipId == ownershipId.Value);
            }

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

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tracking.Dto;
using Tracking.Dto.Pagination;
using TrackingCar.Dto;
using TrackingCar.Dto.user;
using TrackingCar.Interfaces;
using TrakingCar.Data;

namespace TrackingCar.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _secretKey;
        private readonly int _expireTokenHours;
        private readonly int _refreshTokenExpire;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public UserRepository(ApplicationDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(context)
        {
            _context = context;
            _secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _expireTokenHours = configuration.GetValue<int>("Jwt:ExpireHours");
            _refreshTokenExpire = configuration.GetValue<int>("Jwt:RefreshExpireDays");
            _httpContextAccessor = httpContextAccessor; 

        }

        public async Task<bool> IsPasswordCorrect(RegisterationRequestDto registrationRequestDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == registrationRequestDto.UserName);
            if (user == null)
                return false;

            return BCrypt.Net.BCrypt.Verify(registrationRequestDto.Password, user.Password);
        }
        public async Task<bool> IsUniqueUser(string userName)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            return user == null;
        }
        public async Task<UserDto> RegisterUser(RegisterationRequestDto registrationRequestDto)
        {
            try
            {
                // ✅ تحقق من اسم المستخدم موجود مسبقًا
                if (await _context.Users.AnyAsync(u => u.UserName == registrationRequestDto.UserName))
                {
                    throw new Exception("اسم المستخدم موجود بالفعل يجب إدخال اسم فريد");
                }

                string? imageName = null;

                // ✅ حفظ الصورة إذا موجودة
                if (registrationRequestDto.Image != null && registrationRequestDto.Image.Length > 0)
                {
                    var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users");
                    if (!Directory.Exists(uploadsDirectory))
                    {
                        Directory.CreateDirectory(uploadsDirectory);
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(registrationRequestDto.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        throw new Exception("صيغة الملف غير مدعومة. الصيغ المسموحة: .jpg, .jpeg, .png, .gif");
                    }

                    imageName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsDirectory, imageName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await registrationRequestDto.Image.CopyToAsync(stream);
                    }
                }

                // ✅ إنشاء المستخدم
                var user = new User
                {
                    UserName = registrationRequestDto.UserName,
                    Password = BCrypt.Net.BCrypt.HashPassword(registrationRequestDto.Password),
                    FullName = registrationRequestDto.FullName,
                    Number = registrationRequestDto.Number,
                    Statuse = registrationRequestDto.Statuse ?? true, 
                    Role = registrationRequestDto.Role,
                    Image = imageName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                string imageUrl = string.IsNullOrEmpty(user.Image)
                    ? null
                    : $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/uploads/users/{user.Image}";

                

                return new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Number = user.Number,
                    Statuse = user.Statuse,
                    Role = user.Role.HasValue ? user.Role.Value.ToString() : "User", 
                    Image = imageUrl,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل التسجيل: {ex.Message}", ex);
            }
        }
        public async Task<bool> UserExistsAsync(Guid id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id && u.DeletedAt == null);
        }
        public async Task<bool> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
{
        using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return false;
            }

            // ✅ Update basic fields
            existingUser.UserName = userUpdateDto.UserName ?? existingUser.UserName;
            existingUser.FullName = userUpdateDto.FullName ?? existingUser.FullName;
            existingUser.Number = userUpdateDto.Number ?? existingUser.Number;
            existingUser.Statuse = userUpdateDto.Statuse ?? existingUser.Statuse;
            existingUser.Role = userUpdateDto.Role;

            // ✅ Update CreatedAt if explicitly set (rare)
            if (userUpdateDto.CreatedAt.HasValue)
            {
                existingUser.CreatedAt = userUpdateDto.CreatedAt;
            }

                    // ✅ Always update UpdatedAt to now or from dto if present
                    existingUser.UpdatedAt = userUpdateDto.UpdatedAt.HasValue
             ? userUpdateDto.UpdatedAt.Value
             : DateTime.UtcNow;


                    // ✅ Image processing
                    if (userUpdateDto.Image != null && userUpdateDto.Image.Length > 0)
            {
                var uploadsDirectory = Path.Combine("wwwroot", "uploads", "users");

                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                // Delete old image if exists
                if (!string.IsNullOrEmpty(existingUser.Image))
                {
                    var oldImagePath = Path.Combine(uploadsDirectory, existingUser.Image);
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var newImageName = $"{Guid.NewGuid()}{Path.GetExtension(userUpdateDto.Image.FileName)}";
                var newImagePath = Path.Combine(uploadsDirectory, newImageName);

                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    await userUpdateDto.Image.CopyToAsync(stream);
                }

                existingUser.Image = newImageName;
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"Error updating user: {ex.Message}");
            return false;
        }
    }
}
        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (jwtToken.ValidTo < DateTime.UtcNow)
                        return null; // ❌ انتهت صلاحية التوكن

                    var userName = principal.Identity?.Name;
                    if (string.IsNullOrWhiteSpace(userName))
                        return null;

                    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());
                    if (user == null || user.RToken != refreshToken)
                        return null;

                    var tokenResponse = GenerateToken(
                        user.Id,
                        user.UserName,
                        user.Role.ToString(),
                        user.FullName,
                        user.Number,
                        user.Image,
                        user.Statuse
                    );

                    // تحديث RefreshToken في قاعدة البيانات
                    user.RToken = tokenResponse.RefreshToken;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    tokenResponse.FullName = user.FullName ?? "";
                    return tokenResponse;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token refresh error: {ex.Message}");
                return null;
            }
        }
        private LoginResponseDto GenerateToken(
            Guid userId,
            string username,
            string role,
            string fullName,
            string? number,
            string? image,
            bool? statuse)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            // Access Token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }),
                Expires = DateTime.UtcNow.AddHours(_expireTokenHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Refresh Token
            var refreshDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        }),
                Expires = DateTime.UtcNow.AddDays(_refreshTokenExpire),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var accessToken = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = tokenHandler.CreateToken(refreshDescriptor);

            return new LoginResponseDto
            {
                Id = userId,
                User = username,
                Role = role,
                AccessToken = tokenHandler.WriteToken(accessToken),
                RefreshToken = tokenHandler.WriteToken(refreshToken),
                FullName = fullName,
                Number = number,
                Image = image,
                Statuse = statuse
            };
        }

        public async Task<APIResponse> Login(LoginRequestDto loginRequestDto)
        {
            var response = new APIResponse();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(a => a.UserName == loginRequestDto.UserName);

                if (user == null)
                {
                    return new APIResponse
                    {
                        IsSuccess = false,
                        Message = "❌ اسم المستخدم غير موجود."
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.Password))
                {
                    return new APIResponse
                    {
                        IsSuccess = false,
                        Message = "❌ كلمة المرور غير صحيحة."
                    };
                }

                if (!user.Statuse.HasValue || user.Statuse == false)
                {
                    return new APIResponse
                    {
                        IsSuccess = false,
                        Message = "❌ حسابك غير مفعل. يرجى الاتصال بالإدارة."
                    };
                }

                var tokenResponse = GenerateToken(
                    user.Id,
                    user.UserName,
                    user.Role.ToString(),
                    user.FullName,
                    user.Number,
                    user.Image,
                    user.Statuse
                );

                user.RToken = tokenResponse.RefreshToken;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                tokenResponse.FullName = user.FullName ?? string.Empty;

                return new APIResponse
                {
                    IsSuccess = true,
                    Result = tokenResponse,
                    Message = "✅ تم تسجيل الدخول بنجاح."
                };
            }
            catch (Exception ex)
            {
                return new APIResponse
                {
                    IsSuccess = false,
                    Message = $"❌ حدث خطأ أثناء تسجيل الدخول: {ex.Message}"
                };
            }
        }
        //public async Task<bool> LogoutAsync(string refreshToken)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(refreshToken))
        //            return false;

        //        var user = await _context.Users
        //            .FirstOrDefaultAsync(u => u.RToken == refreshToken);

        //        if (user != null)
        //        {
        //            user.RToken = null; // Clear refresh token
        //            user.UpdatedAt = DateTime.UtcNow;
        //            _context.Users.Update(user);
        //            await _context.SaveChangesAsync();
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        // _logger.LogError(ex, "Error during logout");
        //        return false;
        //    }
        //}
        public async Task<IEnumerable<User>> GetRemovedAsync()
        {
            return await _context.Users
                .Where(u => u.DeletedAt != null)
                .OrderByDescending(u => u.DeletedAt)
                .ToListAsync();
        }
        public async Task<PaginatedResponse<UserDto>> GetAllUsersAsync(string? search, int page, int pageSize)
        {
            var query = _context.Users
               .Where(u => u.DeletedAt == null &&
                    u.UserName != "admin" && u.UserName != "admin1");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) ||
                                   u.FullName.Contains(search) ||
                                   u.Number.Contains(search));
            }

            var totalRecords = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Number = u.Number,
                    Statuse = u.Statuse,
                    Role = u.Role.HasValue ? u.Role.Value.ToString() : UserRole.User.ToString(),
                    Image = u.Image,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();


            return new PaginatedResponse<UserDto>
            {
                Data = users,
                Metadata = new PaginationData
                {
                    totalCount = totalRecords,
                    pageSize = pageSize,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
                }
            };
        }
        public async Task<bool> UpdateUserStatusAsync(Guid id, bool status)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Statuse = status; 
            user.UpdatedAt = DateTime.UtcNow;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
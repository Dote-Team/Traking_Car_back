using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TrackingCar.Interfaces;
using TrackingCar.Repositories;
using TrakingCar.Data;
using TrakingCar.Interfaces;
using TrakingCar.Repositories;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.WebHost.UseKestrel()
    .ConfigureKestrel((context, options) =>
    {
        options.Configure(context.Configuration.GetSection("Kestrel"));
    });

var MyAllowSpecificOrigins = "AllowAllOrigins";

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy(MyAllowSpecificOrigins, policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tracking Car System", Version = "v1" });
    c.CustomSchemaIds(type => type.FullName);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; 
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IOwnershipRepository, OwnershipRepository>();






var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// ? Middlewares
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { }
});

app.UseStaticFiles();
app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tracking Car System v1");
    c.RoutePrefix = "swagger";
});

//app.MapGet("/", () => Results.Json(new
//{
//    Message = "🚀 Tracking Car API is running...",
//    Version = "v1",
//    Swagger = "/swagger"
//}));


app.MapGet("/", () =>
{
    return Results.Content(@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <title>Tracking Car API</title>
            <style>
                body { 
                    font-family: Arial, sans-serif; 
                    text-align: center; 
                    margin-top: 50px; 
                    background-color: #f4f4f9;
                }
                h1 { color: #333; }
                p { color: #555; font-size: 18px; }
                a.button {
                    display: inline-block;
                    padding: 12px 24px;
                    margin-top: 20px;
                    background-color: #0078d7;
                    color: white;
                    text-decoration: none;
                    border-radius: 6px;
                    font-size: 16px;
                }
                a.button:hover {
                    background-color: #005fa3;
                }
            </style>
        </head>
        <body>
            <h1>🚗 Tracking Car API</h1>
            <p>API is running successfully. Use the button below to open Swagger UI.</p>
            <a class='button' href='/swagger'>Open Swagger UI</a>
        </body>
        </html>
    ", "text/html");
});


app.MapGet("/users/images/{fileName}", (string fileName) =>
{
    var imagePath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "users", fileName);
    if (!System.IO.File.Exists(imagePath)) return Results.NotFound("User image not found.");
    var extension = Path.GetExtension(fileName).TrimStart('.').ToLower();
    if (!new[] { "jpg", "jpeg", "png", "gif" }.Contains(extension)) return Results.BadRequest("Unsupported image format.");
    return Results.File(imagePath, $"image/{extension}");
});

app.MapControllers();


// Ensure Admin user exists
static async Task EnsureAdminCreated(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Users.Any(u => u.UserName == "admin"))
    {
        var admin = new User
        {
            UserName = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin"),
            FullName = "ahmed abbas",
            Number = "07777777777",
            Statuse = true,
            Role = UserRole.Admin

        };

        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}
await EnsureAdminCreated(app.Services);


static async Task EnsureAdminOneCreated(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!context.Users.Any(u => u.UserName == "admin1"))
    {
        var admin1 = new User
        {
            UserName = "admin1",
            Password = BCrypt.Net.BCrypt.HashPassword("admin1"),
            FullName = "Saif Jabaar",
            Number = "07777777777",
            Statuse = true,
            Role = UserRole.Manager

        };

        context.Users.Add(admin1);
        await context.SaveChangesAsync();
    }
}
await EnsureAdminOneCreated(app.Services);

app.Run();
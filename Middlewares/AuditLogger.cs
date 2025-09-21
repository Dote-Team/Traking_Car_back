using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TrakingCar.Data;
using TrakingCar.Models;

namespace Tracking.Middlewares
{
    using AppLogEntry = LogEntry;

    public interface IAuditLogger
    {
        Task LogAsync(string actionType, PathString path, string requestData, string responseData, int statusCode, string userName, string ip);
    }

    public class AuditLogger : IAuditLogger
    {
        private readonly ApplicationDbContext _context;

        public AuditLogger(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string actionType, PathString path, string requestData, string responseData, int statusCode, string userName, string ip)
        {
            var logEntry = new AppLogEntry
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                IP = ip,
                Method = actionType,
                Path = path,
                RequestBody = requestData,
                ResponseBody = responseData,
                StatusCode = statusCode,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();
        }
    }
}

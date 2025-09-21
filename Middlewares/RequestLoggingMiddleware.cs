using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System;
using TrakingCar.Data;
using TrakingCar.Models;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ApplicationDbContext dbContext)
    {
        var user = context.User?.Identity?.Name ?? "Anonymous";
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var path = context.Request.Path.ToString();
        var method = context.Request.Method;

        // قراءة Request Body (مع تمكين Buffering)
        context.Request.EnableBuffering();
        string requestBody = "";
        try
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }
        catch
        {
            requestBody = "[Could not read request body]";
        }

        // تبديل Response Body لقراءته
        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context); 
        }
        catch (Exception ex)
        {
            // في حالة Exception، سجّل الخطأ في اللوغ ثم أعِد رفعه
            Log.Error(ex, "خطأ خلال معالجة الطلب");
            throw;
        }

        // قراءة Response Body
        string responseBody = "";
        try
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);
        }
        catch
        {
            responseBody = "[Could not read response body]";
        }

        // إعادة الـ Response إلى الجسم الأصلي
        await responseBodyStream.CopyToAsync(originalBodyStream);

        // حفظ اللوغ في Serilog (Console + File)
        Log.Information("Request: {Method} {Path} | User={User}, IP={IP}, Request={RequestBody}, Response={ResponseBody}, StatusCode={StatusCode}",
            method, path, user, ip, Truncate(requestBody, 1000), Truncate(responseBody, 1000), context.Response.StatusCode);

        // حفظ اللوغ في قاعدة البيانات (بـ try-catch لمنع الأعطال في حالة فشل الحفظ)
        try
        {
            var logEntry = new LogEntry
            {
                UserName = user,
                IP = ip,
                Method = method,
                Path = path,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                StatusCode = context.Response.StatusCode,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.LogEntries.Add(logEntry);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "فشل حفظ سجل اللوغ في قاعدة البيانات");
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...(truncated)";
    }
}

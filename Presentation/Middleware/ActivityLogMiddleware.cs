using Application.Services;
using Domain.Enums;
using System.Text.RegularExpressions;

namespace Presentation.Middleware;

public class ActivityLogMiddleware
{
    private readonly RequestDelegate _next;

    public ActivityLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IActivityLogService activityLogService)
    {
        var path = context.Request.Path.Value ?? "";
        var isAdmin = path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase);
        var actionType = isAdmin ? ActivityActionType.AdminAction : ActivityActionType.PageView;

        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "";
        var (device, browser) = ParseUserAgent(userAgent);

        await _next(context);

        if (context.Response.StatusCode < 400)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    int? productId = null;
                    var productMatch = Regex.Match(path, @"/Product/Details/(\d+)");
                    if (productMatch.Success)
                        productId = int.Parse(productMatch.Groups[1].Value);

                    await activityLogService.LogAsync(
                        actionType, userId, ipAddress, device, browser,
                        context.Request.Path + context.Request.QueryString, productId);
                }
                catch { /* Log and continue */ }
            });
        }
    }

    private static (string Device, string Browser) ParseUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return ("Unknown", "Unknown");
        var ua = userAgent.ToLowerInvariant();
        var device = ua.Contains("mobile") && !ua.Contains("ipad") ? "Mobile" : "Desktop";
        var browser = ua.Contains("chrome") ? "Chrome" : ua.Contains("firefox") ? "Firefox" : ua.Contains("safari") ? "Safari" : "Other";
        return (device, browser);
    }
}

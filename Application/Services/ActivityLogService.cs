using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public interface IActivityLogService
{
    Task LogAsync(ActivityActionType actionType, string? userId, string? ipAddress, string? device, string? browser, string? pageUrl, int? productId, CancellationToken cancellationToken = default);
}

public class ActivityLogService : IActivityLogService
{
    private readonly IRepository<ActivityLog> _logRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ActivityLogService(IRepository<ActivityLog> logRepo, IUnitOfWork unitOfWork)
    {
        _logRepo = logRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task LogAsync(ActivityActionType actionType, string? userId, string? ipAddress, string? device, string? browser, string? pageUrl, int? productId, CancellationToken cancellationToken = default)
    {
        await _logRepo.AddAsync(new ActivityLog
        {
            UserId = userId,
            IPAddress = ipAddress,
            Device = device,
            Browser = browser,
            ActionType = actionType,
            PageUrl = pageUrl,
            ProductId = productId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

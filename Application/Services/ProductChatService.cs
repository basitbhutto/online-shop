using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IProductChatService
{
    Task<ProductChatThread?> GetOrCreateThreadAsync(long productId, string buyerUserId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductChatMessage>> GetMessagesAsync(Guid threadId, string userId, bool isAdmin, CancellationToken ct = default);
    Task<ProductChatMessage?> SendMessageAsync(Guid threadId, string userId, string message, bool isFromAdmin, CancellationToken ct = default);
    Task<int> GetUnreadCountForAdminAsync(CancellationToken ct = default);
    Task<int> GetUnreadCountForBuyerAsync(string userId, CancellationToken ct = default);
    Task MarkThreadAsReadAsync(Guid threadId, string userId, bool isAdmin, CancellationToken ct = default);
    Task<IReadOnlyList<(ProductChatThread Thread, int UnreadCount)>> GetAdminThreadsAsync(CancellationToken ct = default);
}

public class ProductChatService : IProductChatService
{
    private readonly IRepository<ProductChatThread> _threadRepo;
    private readonly IRepository<ProductChatMessage> _msgRepo;
    private readonly IUnitOfWork _uow;

    public ProductChatService(IRepository<ProductChatThread> threadRepo, IRepository<ProductChatMessage> msgRepo, IUnitOfWork uow)
    {
        _threadRepo = threadRepo;
        _msgRepo = msgRepo;
        _uow = uow;
    }

    public async Task<ProductChatThread?> GetOrCreateThreadAsync(long productId, string buyerUserId, CancellationToken ct = default)
    {
        var thread = await _threadRepo.FirstOrDefaultAsync(t => t.ProductId == productId && t.BuyerUserId == buyerUserId, ct);
        if (thread != null) return thread;
        thread = new ProductChatThread { Id = Guid.NewGuid(), ProductId = productId, BuyerUserId = buyerUserId };
        await _threadRepo.AddAsync(thread, ct);
        await _uow.SaveChangesAsync(ct);
        return thread;
    }

    public async Task<IReadOnlyList<ProductChatMessage>> GetMessagesAsync(Guid threadId, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var thread = await _threadRepo.Query()
            .Include(t => t.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(t => t.Id == threadId, ct);
        if (thread == null) return Array.Empty<ProductChatMessage>();
        if (!isAdmin && thread.BuyerUserId != userId) return Array.Empty<ProductChatMessage>();
        return thread.Messages.OrderBy(m => m.CreatedAt).ToList();
    }

    public async Task<ProductChatMessage?> SendMessageAsync(Guid threadId, string userId, string message, bool isFromAdmin, CancellationToken ct = default)
    {
        var thread = await _threadRepo.Query().FirstOrDefaultAsync(t => t.Id == threadId, ct);
        if (thread == null) return null;
        if (!isFromAdmin && thread.BuyerUserId != userId) return null;
        if (string.IsNullOrWhiteSpace(message)) return null;
        var msg = new ProductChatMessage { Id = Guid.NewGuid(), ThreadId = threadId, UserId = userId, Message = message.Trim(), IsFromAdmin = isFromAdmin };
        await _msgRepo.AddAsync(msg, ct);
        await _uow.SaveChangesAsync(ct);
        return msg;
    }

    public async Task<int> GetUnreadCountForAdminAsync(CancellationToken ct = default)
    {
        return await _msgRepo.Query()
            .Where(m => !m.IsFromAdmin && !m.IsRead)
            .CountAsync(ct);
    }

    public async Task<int> GetUnreadCountForBuyerAsync(string userId, CancellationToken ct = default)
    {
        var threadIds = await _threadRepo.Query().Where(t => t.BuyerUserId == userId).Select(t => t.Id).ToListAsync(ct);
        return await _msgRepo.Query()
            .Where(m => m.IsFromAdmin && !m.IsRead && threadIds.Contains(m.ThreadId))
            .CountAsync(ct);
    }

    public async Task MarkThreadAsReadAsync(Guid threadId, string userId, bool isAdmin, CancellationToken ct = default)
    {
        var thread = await _threadRepo.GetByIdAsync(threadId, ct);
        if (thread == null) return;
        if (!isAdmin && thread.BuyerUserId != userId) return;
        var msgs = await _msgRepo.Query()
            .Where(m => m.ThreadId == threadId && (isAdmin ? !m.IsFromAdmin : m.IsFromAdmin))
            .ToListAsync(ct);
        foreach (var m in msgs) { m.IsRead = true; _msgRepo.Update(m); }
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<(ProductChatThread Thread, int UnreadCount)>> GetAdminThreadsAsync(CancellationToken ct = default)
    {
        var threads = await _threadRepo.Query()
            .Include(t => t.Product)
            .Include(t => t.Buyer)
            .Include(t => t.Messages)
            .OrderByDescending(t => t.Id)
            .ToListAsync(ct);
        var result = new List<(ProductChatThread, int)>();
        foreach (var t in threads)
        {
            var unread = t.Messages.Count(m => !m.IsFromAdmin && !m.IsRead);
            result.Add((t, unread));
        }
        return result;
    }
}

using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;

namespace Application.Services;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(string userId, string shippingAddress, string phoneNumber, string? preferredDeliveryTime, List<(long ProductId, long? VariantId, int Quantity, decimal Price)> items, CancellationToken cancellationToken = default);
    Task<Order?> GetOrderByIdAsync(long orderId, string? userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(long orderId, string userId, bool isAdmin, string? notes, CancellationToken cancellationToken = default);
}

public class OrderService : IOrderService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderItem> _orderItemRepo;
    private readonly IRepository<OrderStatusHistory> _historyRepo;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IRepository<Order> orderRepo,
        IRepository<OrderItem> orderItemRepo,
        IRepository<OrderStatusHistory> historyRepo,
        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _historyRepo = historyRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<Order?> CreateOrderAsync(string userId, string shippingAddress, string phoneNumber, string? preferredDeliveryTime, List<(long ProductId, long? VariantId, int Quantity, decimal Price)> items, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(shippingAddress)) return null;
        if (items.Count == 0) return null;

        var totalAmount = items.Sum(i => i.Quantity * i.Price);
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.PendingConfirmation,
            TotalAmount = totalAmount,
            ShippingAddress = shippingAddress,
            City = AppConstants.KarachiCity,
            PhoneNumber = phoneNumber ?? string.Empty,
            PreferredDeliveryTime = preferredDeliveryTime
        };

        await _orderRepo.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (productId, variantId, quantity, price) in items)
        {
            await _orderItemRepo.AddAsync(new OrderItem
            {
                OrderId = order.Id,
                ProductId = productId,
                VariantId = variantId,
                Quantity = quantity,
                Price = price
            }, cancellationToken);
        }

        await _historyRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = (int)OrderStatus.PendingConfirmation,
            ChangedByUserId = userId,
            Notes = "Order placed"
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(long orderId, string? userId, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.Query()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedDate))
            .Include(o => o.DeliveryAssignment)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order == null) return null;
        if (userId != null && order.UserId != userId) return null; // Auth check for buyers
        return order;
    }

    public async Task<IReadOnlyList<Order>> GetUserOrdersAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _orderRepo.Query()
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedDate))
            .OrderByDescending(o => o.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CancelOrderAsync(long orderId, string userId, bool isAdmin, string? notes, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(orderId, cancellationToken);
        if (order == null) return false;

        if (!isAdmin && order.UserId != userId) return false;

        if (isAdmin)
        {
            if (order.Status == OrderStatus.Delivered) return false;
        }
        else
        {
            var cancellableStatuses = new[] { OrderStatus.PendingConfirmation, OrderStatus.AdminReview, OrderStatus.Confirmed, OrderStatus.Preparing };
            if (!cancellableStatuses.Contains(order.Status)) return false;
        }

        order.Status = OrderStatus.Cancelled;
        _orderRepo.Update(order);

        await _historyRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = orderId,
            Status = (int)OrderStatus.Cancelled,
            ChangedByUserId = userId,
            Notes = notes ?? (isAdmin ? "Cancelled by admin" : "Cancelled by customer")
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

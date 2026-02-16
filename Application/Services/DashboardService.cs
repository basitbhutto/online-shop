using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MonthlySalesDto>> GetMonthlySalesAsync(int year, CancellationToken cancellationToken = default);
}

public record DashboardStatsDto(
    int TotalOrders,
    decimal TotalRevenue,
    decimal TotalPurchaseCost,
    decimal TotalDiscount,
    decimal NetProfit
);

public record MonthlySalesDto(int Month, decimal Revenue, int OrderCount);

public class DashboardService : IDashboardService
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderItem> _orderItemRepo;
    private readonly IRepository<Product> _productRepo;

    public DashboardService(
        IRepository<Order> orderRepo,
        IRepository<OrderItem> orderItemRepo,
        IRepository<Product> productRepo)
    {
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _productRepo = productRepo;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var deliveredOrders = _orderRepo.Query().Where(o => o.Status == OrderStatus.Delivered);
        var totalOrders = await deliveredOrders.CountAsync(cancellationToken);
        var totalRevenue = await deliveredOrders.SumAsync(o => o.TotalAmount, cancellationToken);

        var orderIds = await deliveredOrders.Select(o => o.Id).ToListAsync(cancellationToken);
        if (orderIds.Count == 0)
            return new DashboardStatsDto(0, 0, 0, 0, 0);

        var orderItems = await _orderItemRepo.Query()
            .Where(oi => orderIds.Contains(oi.OrderId))
            .Include(oi => oi.Product)
            .ToListAsync(cancellationToken);

        var totalPurchaseCost = orderItems.Sum(oi => oi.Product != null ? oi.Quantity * oi.Product.PurchasePrice : 0);
        var totalSaleAmount = orderItems.Sum(oi => oi.Quantity * oi.Price);
        var totalDiscount = totalSaleAmount - totalRevenue;
        var netProfit = totalRevenue - totalPurchaseCost;

        return new DashboardStatsDto(totalOrders, totalRevenue, totalPurchaseCost, totalDiscount, netProfit);
    }

    public async Task<IReadOnlyList<MonthlySalesDto>> GetMonthlySalesAsync(int year, CancellationToken cancellationToken = default)
    {
        List<MonthlySalesDto> orders;
        try
        {
            orders = await _orderRepo.Query()
                .Where(o => o.Status == OrderStatus.Delivered && o.CreatedDate.Year == year)
                .GroupBy(o => o.CreatedDate.Month)
                .Select(g => new MonthlySalesDto(g.Key, g.Sum(o => o.TotalAmount), g.Count()))
                .OrderBy(x => x.Month)
                .ToListAsync(cancellationToken);
        }
        catch
        {
            orders = new List<MonthlySalesDto>();
        }

        for (var m = 1; m <= 12; m++)
        {
            if (orders.All(x => x.Month != m))
                orders.Add(new MonthlySalesDto(m, 0, 0));
        }

        return orders.OrderBy(x => x.Month).ToList();
    }
}

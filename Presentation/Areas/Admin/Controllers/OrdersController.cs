using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Presentation.Areas.Admin.ViewModels;
using Shared.Constants;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class OrdersController : Controller
{
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<DeliveryAssignment> _deliveryRepo;
    private readonly IRepository<OrderStatusHistory> _historyRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;

    public OrdersController(
        IRepository<Order> orderRepo,
        IRepository<DeliveryAssignment> deliveryRepo,
        IRepository<OrderStatusHistory> historyRepo,
        IUnitOfWork unitOfWork,
        IOrderService orderService)
    {
        _orderRepo = orderRepo;
        _deliveryRepo = deliveryRepo;
        _historyRepo = historyRepo;
        _unitOfWork = unitOfWork;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken = default)
    {
        IQueryable<Order> query = _orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
            query = query.Where(o => o.Status == orderStatus);

        var orders = await query.OrderByDescending(o => o.CreatedDate).ToListAsync(cancellationToken);
        return View(orders);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
            .Include(o => o.StatusHistory.OrderByDescending(h => h.ChangedDate))
            .Include(o => o.DeliveryAssignment)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (order == null) return NotFound();
        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, string? notes, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order == null) return NotFound();

        order.Status = status;
        _orderRepo.Update(order);

        await _historyRepo.AddAsync(new OrderStatusHistory
        {
            OrderId = id,
            Status = (int)status,
            ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
            Notes = notes
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignDelivery(int id, string deliveryBoyName, string phoneNumber, string vehicleType, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order == null) return NotFound();

        var existing = await _deliveryRepo.FirstOrDefaultAsync(d => d.OrderId == id, cancellationToken);
        if (existing != null)
        {
            existing.DeliveryBoyName = deliveryBoyName;
            existing.PhoneNumber = phoneNumber;
            existing.VehicleType = vehicleType;
            _deliveryRepo.Update(existing);
        }
        else
        {
            await _deliveryRepo.AddAsync(new DeliveryAssignment
            {
                OrderId = id,
                DeliveryBoyName = deliveryBoyName,
                PhoneNumber = phoneNumber,
                VehicleType = vehicleType
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkDelivered(int id, CancellationToken cancellationToken = default)
    {
        var delivery = await _deliveryRepo.FirstOrDefaultAsync(d => d.OrderId == id, cancellationToken);
        if (delivery != null)
        {
            delivery.DeliveredDate = DateTime.UtcNow;
            _deliveryRepo.Update(delivery);
        }

        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order != null)
        {
            order.Status = OrderStatus.Delivered;
            _orderRepo.Update(order);
            await _historyRepo.AddAsync(new OrderStatusHistory
            {
                OrderId = id,
                Status = (int)OrderStatus.Delivered,
                ChangedByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value,
                Notes = "Marked as delivered"
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int id, string? notes, CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        var ok = await _orderService.CancelOrderAsync(id, userId, isAdmin: true, notes, cancellationToken);
        if (!ok) TempData["Error"] = "Order cannot be cancelled (e.g. already delivered).";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDeliveryTime(int id, string preferredDeliveryTime, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepo.GetByIdAsync(id, cancellationToken);
        if (order == null) return NotFound();
        order.PreferredDeliveryTime = string.IsNullOrWhiteSpace(preferredDeliveryTime) ? null : preferredDeliveryTime;
        _orderRepo.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }
}

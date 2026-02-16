using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface ICartService
{
    Task<IReadOnlyList<CartItem>> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCartCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<CartItem?> AddToCartAsync(string userId, int productId, int? variantId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromCartAsync(string userId, int cartItemId, CancellationToken cancellationToken = default);
    Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}

public class CartService : ICartService
{
    private readonly IRepository<CartItem> _cartRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(IRepository<CartItem> cartRepo, IUnitOfWork unitOfWork)
    {
        _cartRepo = cartRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CartItem>> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _cartRepo.Query()
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p!.Images.OrderBy(i => i.SortOrder))
            .Include(c => c.Product.Category)
            .Include(c => c.Variant)
            .OrderByDescending(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCartCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _cartRepo.Query()
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity, cancellationToken);
    }

    public async Task<CartItem?> AddToCartAsync(string userId, int productId, int? variantId, int quantity, CancellationToken cancellationToken = default)
    {
        var existing = await _cartRepo.FirstOrDefaultAsync(
            c => c.UserId == userId && c.ProductId == productId && c.VariantId == variantId, cancellationToken);

        if (existing != null)
        {
            existing.Quantity += quantity;
            _cartRepo.Update(existing);
        }
        else
        {
            var item = new CartItem { UserId = userId, ProductId = productId, VariantId = variantId, Quantity = quantity };
            await _cartRepo.AddAsync(item, cancellationToken);
            existing = item;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId, CancellationToken cancellationToken = default)
    {
        var item = await _cartRepo.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId, cancellationToken);
        if (item == null) return false;
        _cartRepo.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _cartRepo.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId, cancellationToken);
        if (item == null || quantity <= 0) return false;
        item.Quantity = quantity;
        _cartRepo.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var items = await _cartRepo.FindAsync(c => c.UserId == userId, cancellationToken);
        foreach (var item in items) _cartRepo.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

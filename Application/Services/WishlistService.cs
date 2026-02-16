using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface IWishlistService
{
    Task<IReadOnlyList<Wishlist>> GetWishlistAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> AddToWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFromWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> IsInWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default);
}

public class WishlistService : IWishlistService
{
    private readonly IRepository<Wishlist> _wishlistRepo;
    private readonly IUnitOfWork _unitOfWork;

    public WishlistService(IRepository<Wishlist> wishlistRepo, IUnitOfWork unitOfWork)
    {
        _wishlistRepo = wishlistRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Wishlist>> GetWishlistAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _wishlistRepo.Query()
            .Where(w => w.UserId == userId)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Category)
            .Include(w => w.Product)
                .ThenInclude(p => p!.Images.OrderBy(i => i.SortOrder))
            .OrderByDescending(w => w.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AddToWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        var existing = await _wishlistRepo.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
        if (existing != null) return true;

        await _wishlistRepo.AddAsync(new Wishlist { UserId = userId, ProductId = productId }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveFromWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        var item = await _wishlistRepo.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);
        if (item == null) return false;
        _wishlistRepo.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsInWishlistAsync(string userId, int productId, CancellationToken cancellationToken = default)
    {
        return await _wishlistRepo.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken) != null;
    }
}

using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Application.Services;

public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllForAdminAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdForAdminAsync(int id, CancellationToken cancellationToken = default);
    Task<Product> CreateAsync(string name, string sku, int categoryId, decimal purchasePrice, decimal salePrice, decimal? discountPrice, int stock, string? description, List<string>? imageUrls = null, List<(string Key, string Value)>? specifications = null, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, string sku, int categoryId, decimal purchasePrice, decimal salePrice, decimal? discountPrice, int stock, string? description, EntityStatus status, List<string>? imageUrls = null, List<(string Key, string Value)>? specifications = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDetailDto?> GetProductDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListDto>> GetProductsByCategoryAsync(int categoryId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListDto>> SearchProductsAsync(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductListDto>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
}

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<ProductImage> _imageRepo;
    private readonly IRepository<ProductSpecification> _specRepo;
    private readonly IUnitOfWork _uow;

    public ProductService(IRepository<Product> productRepo, IRepository<ProductImage> imageRepo, IRepository<ProductSpecification> specRepo, IUnitOfWork uow)
    {
        _productRepo = productRepo;
        _imageRepo = imageRepo;
        _specRepo = specRepo;
        _uow = uow;
    }

    public async Task<IReadOnlyList<Product>> GetAllForAdminAsync(CancellationToken cancellationToken = default)
    {
        return await _productRepo.Query().Include(p => p.Category).Include(p => p.Images.OrderBy(i => i.SortOrder)).OrderByDescending(p => p.CreatedDate).ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specifications)
            .Include(p => p.Variants)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status == EntityStatus.Active, cancellationToken);
    }

    public async Task<Product?> GetByIdForAdminAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Specifications)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Product> CreateAsync(string name, string sku, int categoryId, decimal purchasePrice, decimal salePrice, decimal? discountPrice, int stock, string? description, List<string>? imageUrls = null, List<(string Key, string Value)>? specifications = null, CancellationToken cancellationToken = default)
    {
        var product = new Product { Name = name, SKU = sku, CategoryId = categoryId, PurchasePrice = purchasePrice, SalePrice = salePrice, DiscountPrice = discountPrice, Stock = stock, Description = description, Status = EntityStatus.Active };
        await _productRepo.AddAsync(product, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        var order = 0;
        foreach (var url in imageUrls ?? new List<string>())
        {
            if (string.IsNullOrWhiteSpace(url)) continue;
            await _imageRepo.AddAsync(new ProductImage { ProductId = product.Id, ImageUrl = url, SortOrder = order++ }, cancellationToken);
        }
        foreach (var (key, value) in specifications ?? new List<(string, string)>())
        {
            if (string.IsNullOrWhiteSpace(key)) continue;
            await _specRepo.AddAsync(new ProductSpecification { ProductId = product.Id, SpecKey = key.Trim(), SpecValue = value?.Trim() ?? "" }, cancellationToken);
        }
        if ((imageUrls?.Count ?? 0) > 0 || (specifications?.Count ?? 0) > 0)
            await _uow.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task UpdateAsync(int id, string name, string sku, int categoryId, decimal purchasePrice, decimal salePrice, decimal? discountPrice, int stock, string? description, EntityStatus status, List<string>? imageUrls = null, List<(string Key, string Value)>? specifications = null, CancellationToken cancellationToken = default)
    {
        var p = await _productRepo.Query().Include(x => x.Images).Include(x => x.Specifications).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (p == null) return;
        p.Name = name; p.SKU = sku; p.CategoryId = categoryId; p.PurchasePrice = purchasePrice; p.SalePrice = salePrice; p.DiscountPrice = discountPrice; p.Stock = stock; p.Description = description; p.Status = status;
        _productRepo.Update(p);
        if (imageUrls != null)
        {
            foreach (var img in p.Images.ToList()) _imageRepo.Remove(img);
            var order = 0;
            foreach (var url in imageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
                await _imageRepo.AddAsync(new ProductImage { ProductId = id, ImageUrl = url, SortOrder = order++ }, cancellationToken);
        }
        if (specifications != null)
        {
            foreach (var s in p.Specifications.ToList()) _specRepo.Remove(s);
            foreach (var (key, value) in specifications.Where(x => !string.IsNullOrWhiteSpace(x.Key)))
                await _specRepo.AddAsync(new ProductSpecification { ProductId = id, SpecKey = key.Trim(), SpecValue = value?.Trim() ?? "" }, cancellationToken);
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var p = await _productRepo.GetByIdAsync(id, cancellationToken);
        if (p != null) { _productRepo.Remove(p); await _uow.SaveChangesAsync(cancellationToken); }
    }

    public async Task<ProductDetailDto?> GetProductDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await GetByIdAsync(id, cancellationToken);
        if (product == null) return null;

        var specs = (product.Specifications ?? new List<ProductSpecification>()).Select(s => new ProductSpecificationDto(s.SpecKey, s.SpecValue)).ToList();
        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.SKU,
            product.Category.Name,
            product.PurchasePrice,
            product.SalePrice,
            product.DiscountPrice,
            product.Stock + product.Variants.Sum(v => v.Stock),
            product.Description,
            product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
            product.AttributeValues.Select(av => new ProductAttributeValueDto(av.Attribute.Name, av.Value)).ToList(),
            specs,
            product.Variants.Select(v => new ProductVariantDto(v.Id, v.VariantCombination, v.Stock, v.PriceOverride, v.SKU)).ToList()
        );
    }

    public async Task<IReadOnlyList<ProductListDto>> GetProductsByCategoryAsync(int categoryId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await _productRepo.Query()
            .Where(p => p.CategoryId == categoryId && p.Status == EntityStatus.Active)
            .OrderByDescending(p => p.CreatedDate)
            .Skip(skip)
            .Take(take)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Category.Name,
                p.SalePrice,
                p.DiscountPrice,
                p.Stock + p.Variants.Sum(v => v.Stock),
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductListDto>> SearchProductsAsync(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        var query = _productRepo.Query().Where(p => p.Status == EntityStatus.Active);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm) || p.SKU.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var priceQuery = query;
        if (minPrice.HasValue)
            priceQuery = priceQuery.Where(p => (p.DiscountPrice ?? p.SalePrice) >= minPrice.Value);
        if (maxPrice.HasValue)
            priceQuery = priceQuery.Where(p => (p.DiscountPrice ?? p.SalePrice) <= maxPrice.Value);

        return await priceQuery
            .OrderByDescending(p => p.CreatedDate)
            .Skip(skip)
            .Take(take)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Category.Name,
                p.SalePrice,
                p.DiscountPrice,
                p.Stock + p.Variants.Sum(v => v.Stock),
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()!
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProductListDto>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids?.Where(id => id > 0).Distinct().Take(100).ToList() ?? new List<int>();
        if (idList.Count == 0) return new List<ProductListDto>();
        var results = await _productRepo.Query()
            .Where(p => idList.Contains(p.Id) && p.Status == EntityStatus.Active)
            .Select(p => new ProductListDto(
                p.Id,
                p.Name,
                p.SKU,
                p.Category!.Name,
                p.SalePrice,
                p.DiscountPrice,
                p.Stock + p.Variants.Sum(v => v.Stock),
                p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
            ))
            .ToListAsync(cancellationToken);
        return results.OrderBy(p => idList.IndexOf(p.Id)).ToList();
    }
}

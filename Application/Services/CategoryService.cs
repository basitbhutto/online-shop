using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetChildCategoriesAsync(int parentId, CancellationToken cancellationToken = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllWithHierarchyAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(string name, int? parentId, string? imageUrl, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, string name, string? imageUrl, EntityStatus status, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepo;
    private readonly IUnitOfWork _uow;

    public CategoryService(IRepository<Category> categoryRepo, IUnitOfWork uow)
    {
        _categoryRepo = categoryRepo;
        _uow = uow;
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Where(c => c.ParentId == null && c.Status == EntityStatus.Active)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetChildCategoriesAsync(int parentId, CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Where(c => c.ParentId == parentId && c.Status == EntityStatus.Active)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug && c.Status == EntityStatus.Active, cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Include(c => c.CategoryAttributes)
                .ThenInclude(ca => ca.Attribute)
                    .ThenInclude(a => a.Options)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllWithHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepo.Query()
            .Include(c => c.Parent)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Category> CreateAsync(string name, int? parentId, string? imageUrl, CancellationToken cancellationToken = default)
    {
        var slug = name.ToLowerInvariant().Replace(" ", "-");
        var slugBase = slug;
        var i = 1;
        while (await _categoryRepo.FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken) != null)
            slug = slugBase + "-" + (i++);
        var cat = new Category { Name = name, Slug = slug, ParentId = parentId, ImageUrl = imageUrl, Status = EntityStatus.Active };
        await _categoryRepo.AddAsync(cat, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return cat;
    }

    public async Task UpdateAsync(int id, string name, string? imageUrl, EntityStatus status, CancellationToken cancellationToken = default)
    {
        var cat = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (cat == null) return;
        cat.Name = name;
        cat.Status = status;
        if (imageUrl != null) cat.ImageUrl = imageUrl;
        if (string.IsNullOrEmpty(cat.Slug)) cat.Slug = name.ToLowerInvariant().Replace(" ", "-");
        _categoryRepo.Update(cat);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var cat = await _categoryRepo.GetByIdAsync(id, cancellationToken);
        if (cat != null)
        {
            _categoryRepo.Remove(cat);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}

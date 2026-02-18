using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Application.Services;

public interface IAttributeService
{
    Task<IReadOnlyList<CategoryAttributeDto>> GetAttributesByCategoryIdAsync(long categoryId, CancellationToken cancellationToken = default);
}

public class AttributeService : IAttributeService
{
    private readonly IRepository<CategoryAttribute> _categoryAttributeRepo;

    public AttributeService(IRepository<CategoryAttribute> categoryAttributeRepo)
    {
        _categoryAttributeRepo = categoryAttributeRepo;
    }

    public async Task<IReadOnlyList<CategoryAttributeDto>> GetAttributesByCategoryIdAsync(long categoryId, CancellationToken cancellationToken = default)
    {
        return await _categoryAttributeRepo.Query()
            .Where(ca => ca.CategoryId == categoryId)
            .Include(ca => ca.Attribute)
                .ThenInclude(a => a.Options)
            .Select(ca => new CategoryAttributeDto(
                ca.Attribute.Id,
                ca.Attribute.Name,
                ca.Attribute.FieldType,
                ca.Attribute.IsRequired,
                ca.Attribute.Options.Select(o => o.Value).ToList()
            ))
            .ToListAsync(cancellationToken);
    }
}

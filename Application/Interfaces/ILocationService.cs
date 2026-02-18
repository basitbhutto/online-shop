using Shared.DTOs;

namespace Application.Interfaces;

public interface ILocationService
{
    Task<IReadOnlyList<LocationOptionDto>> GetRootsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationOptionDto>> GetChildrenAsync(long? parentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationWithPathDto>> GetLocationsWithProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocationWithPathDto>> SearchLocationsAsync(string? q, CancellationToken cancellationToken = default);
    Task<LocationOptionDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<long> CreateAsync(string name, long? parentId, decimal? latitude, decimal? longitude, int displayOrder, CancellationToken cancellationToken = default);
    Task UpdateAsync(long id, string name, long? parentId, decimal? latitude, decimal? longitude, int displayOrder, CancellationToken cancellationToken = default);
}

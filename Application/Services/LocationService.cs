using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;

namespace Application.Services;

public class LocationService : ILocationService
{
    private readonly IRepository<Location> _repo;
    private readonly IRepository<Product> _productRepo;
    private readonly IUnitOfWork _uow;

    public LocationService(IRepository<Location> repo, IRepository<Product> productRepo, IUnitOfWork uow)
    {
        _repo = repo;
        _productRepo = productRepo;
        _uow = uow;
    }

    public async Task<IReadOnlyList<LocationOptionDto>> GetRootsAsync(CancellationToken cancellationToken = default)
    {
        return await GetChildrenAsync(null, cancellationToken);
    }

    public async Task<IReadOnlyList<LocationOptionDto>> GetChildrenAsync(long? parentId, CancellationToken cancellationToken = default)
    {
        var query = _repo.Query().Where(l => l.ParentId == parentId).OrderBy(l => l.DisplayOrder).ThenBy(l => l.Name);
        return await query
            .Select(l => new LocationOptionDto(l.Id, l.Name, l.ParentId, l.Latitude, l.Longitude))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LocationWithPathDto>> GetLocationsWithProductsAsync(CancellationToken cancellationToken = default)
    {
        var locationIds = await _productRepo.Query()
            .Where(p => p.LocationId != null && p.Status == Domain.Enums.EntityStatus.Active)
            .Select(p => p.LocationId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
        if (locationIds.Count == 0) return new List<LocationWithPathDto>();
        var locations = await _repo.Query()
            .Where(l => locationIds.Contains(l.Id))
            .Select(l => new LocationWithPathDto(l.Id, l.Name, l.FullPath ?? l.Name))
            .ToListAsync(cancellationToken);
        return locations.OrderBy(l => l.FullPath).ToList();
    }

    public async Task<IReadOnlyList<LocationWithPathDto>> SearchLocationsAsync(string? q, CancellationToken cancellationToken = default)
    {
        var query = _repo.Query().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(l => (l.FullPath != null && l.FullPath.ToLower().Contains(term)) || l.Name.ToLower().Contains(term));
        }
        return await query
            .OrderBy(l => l.FullPath ?? l.Name)
            .Take(200)
            .Select(l => new LocationWithPathDto(l.Id, l.Name, l.FullPath ?? l.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<LocationOptionDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var loc = await _repo.GetByIdAsync(id, cancellationToken);
        return loc == null ? null : new LocationOptionDto(loc.Id, loc.Name, loc.ParentId, loc.Latitude, loc.Longitude);
    }

    private static string BuildFullPath(Location loc, IReadOnlyDictionary<long, Location> byId)
    {
        var parts = new List<string> { loc.Name };
        var current = loc;
        while (current.ParentId.HasValue && byId.TryGetValue(current.ParentId.Value, out var parent))
        {
            parts.Insert(0, parent.Name);
            current = parent;
        }
        return string.Join(", ", parts);
    }

    public async Task<long> CreateAsync(string name, long? parentId, decimal? latitude, decimal? longitude, int displayOrder, CancellationToken cancellationToken = default)
    {
        var loc = new Location { Name = name.Trim(), ParentId = parentId, Latitude = latitude, Longitude = longitude, DisplayOrder = displayOrder };
        await _repo.AddAsync(loc, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        await UpdateFullPathRecursive(loc.Id, cancellationToken);
        return loc.Id;
    }

    public async Task UpdateAsync(long id, string name, long? parentId, decimal? latitude, decimal? longitude, int displayOrder, CancellationToken cancellationToken = default)
    {
        var loc = await _repo.GetByIdAsync(id, cancellationToken);
        if (loc == null) return;
        loc.Name = name.Trim();
        loc.ParentId = parentId;
        loc.Latitude = latitude;
        loc.Longitude = longitude;
        loc.DisplayOrder = displayOrder;
        _repo.Update(loc);
        await _uow.SaveChangesAsync(cancellationToken);
        await UpdateFullPathRecursive(id, cancellationToken);
    }

    private async Task UpdateFullPathRecursive(long locationId, CancellationToken cancellationToken)
    {
        var all = await _repo.Query().AsNoTracking().ToListAsync(cancellationToken);
        var byId = all.ToDictionary(x => x.Id);
        var toUpdate = new List<long>();
        var stack = new Stack<long>();
        stack.Push(locationId);
        while (stack.Count > 0)
        {
            var id = stack.Pop();
            toUpdate.Add(id);
            foreach (var c in all.Where(x => x.ParentId == id))
                stack.Push(c.Id);
        }
        foreach (var id in toUpdate.Distinct())
        {
            if (!byId.TryGetValue(id, out var l)) continue;
            var path = BuildFullPath(l, byId);
            var entity = await _repo.GetByIdAsync(id, cancellationToken);
            if (entity != null) { entity.FullPath = path; _repo.Update(entity); }
        }
        await _uow.SaveChangesAsync(cancellationToken);
    }
}

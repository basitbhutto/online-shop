using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
[Route("Admin/api/[controller]")]
[ApiController]
public class LocationsApiController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsApiController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("roots")]
    public async Task<IActionResult> GetRoots(CancellationToken ct)
    {
        var list = await _locationService.GetRootsAsync(ct);
        return Ok(list.Select(x => new { x.Id, x.Name, x.ParentId, x.Latitude, x.Longitude }));
    }

    [HttpGet("children")]
    public async Task<IActionResult> GetChildren([FromQuery] int? parentId, CancellationToken ct)
    {
        var list = await _locationService.GetChildrenAsync(parentId, ct);
        return Ok(list.Select(x => new { x.Id, x.Name, x.ParentId, x.Latitude, x.Longitude }));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var list = await _locationService.SearchLocationsAsync(q, ct);
        return Ok(list.Select(x => new { x.Id, x.Name, x.FullPath }));
    }
}

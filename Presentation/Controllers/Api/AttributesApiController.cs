using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AttributesController : ControllerBase
{
    private readonly IAttributeService _attributeService;

    public AttributesController(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    [HttpGet("{categoryId:int}")]
    public async Task<IActionResult> GetByCategory(int categoryId, CancellationToken cancellationToken = default)
    {
        var attributes = await _attributeService.GetAttributesByCategoryIdAsync(categoryId, cancellationToken);
        return Ok(attributes);
    }
}

using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminAccess")]
public class ProductChatController : Controller
{
    private readonly IProductChatService _chatService;

    public ProductChatController(IProductChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var threads = await _chatService.GetAdminThreadsAsync(ct);
        return View(threads);
    }

    public async Task<IActionResult> Thread(Guid id, CancellationToken ct)
    {
        var threads = await _chatService.GetAdminThreadsAsync(ct);
        var thread = threads.FirstOrDefault(t => t.Thread.Id == id).Thread;
        if (thread == null) return NotFound();
        ViewBag.Threads = threads;
        ViewBag.CurrentThread = thread;
        return View("Index");
    }
}

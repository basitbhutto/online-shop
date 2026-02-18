using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Presentation.Hubs;

namespace Presentation.Controllers.Api;

public class SendMessageRequest { public string? Message { get; set; } }

[ApiController]
[Route("api/chat")]
[Authorize]
public class ProductChatApiController : ControllerBase
{
    private readonly IProductChatService _chatService;
    private readonly IHubContext<ProductChatHub> _hubContext;

    public ProductChatApiController(IProductChatService chatService, IHubContext<ProductChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    [HttpPost("thread/{productId}")]
    public async Task<IActionResult> GetOrCreateThread(long productId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var thread = await _chatService.GetOrCreateThreadAsync(productId, userId, ct);
        return Ok(new { threadId = thread?.Id });
    }

    [HttpGet("thread/{threadId:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid threadId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("AdminStaff");
        var msgs = await _chatService.GetMessagesAsync(threadId, userId, isAdmin, ct);
        return Ok(msgs.Select(m => new { m.Id, m.Message, m.IsFromAdmin, m.CreatedAt }));
    }

    [HttpPost("thread/{threadId:guid}/send")]
    public async Task<IActionResult> SendMessage(Guid threadId, [FromBody] SendMessageRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(req?.Message)) return BadRequest();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("AdminStaff");
        var msg = await _chatService.SendMessageAsync(threadId, userId, req.Message, isAdmin, ct);
        if (msg == null) return NotFound();
        // Broadcast so all connected clients (including buyer) receive in real-time
        await _hubContext.Clients.Group("thread_" + threadId).SendAsync("ReceiveMessage", new
        {
            id = msg.Id,
            message = msg.Message,
            isFromAdmin = msg.IsFromAdmin,
            senderName = User.Identity?.Name ?? "User",
            senderUserId = userId,
            createdAt = msg.CreatedAt
        }, ct);
        return Ok(new { msg.Id, msg.Message, msg.IsFromAdmin, msg.CreatedAt });
    }

    [HttpPost("thread/{threadId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid threadId, CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("AdminStaff");
        await _chatService.MarkThreadAsReadAsync(threadId, userId, isAdmin, ct);
        return Ok();
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken ct)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var isAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("AdminStaff");
        var count = isAdmin
            ? await _chatService.GetUnreadCountForAdminAsync(ct)
            : await _chatService.GetUnreadCountForBuyerAsync(userId, ct);
        return Ok(new { count });
    }
}

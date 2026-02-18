using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Presentation.Hubs;

[Authorize]
public class ProductChatHub : Hub
{
    private readonly IProductChatService _chatService;

    public ProductChatHub(IProductChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task JoinThread(Guid threadId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "thread_" + threadId);
    }

    public async Task LeaveThread(Guid threadId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "thread_" + threadId);
    }

    public async Task SendMessage(Guid threadId, string message)
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return;
        var isAdmin = Context.User?.IsInRole("SuperAdmin") == true || Context.User?.IsInRole("AdminStaff") == true;
        var msg = await _chatService.SendMessageAsync(threadId, userId, message, isAdmin, Context.ConnectionAborted);
        if (msg != null)
        {
            await Clients.Group("thread_" + threadId).SendAsync("ReceiveMessage", new
            {
                id = msg.Id,
                message = msg.Message,
                isFromAdmin = msg.IsFromAdmin,
                senderName = Context.User?.Identity?.Name ?? "User",
                senderUserId = userId,
                createdAt = msg.CreatedAt
            });
        }
    }
}

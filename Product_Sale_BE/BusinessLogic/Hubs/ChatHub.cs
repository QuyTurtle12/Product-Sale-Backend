// Hubs/ChatHub.cs
using DataAccess.DTOs.ChatDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BusinessLogic.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            // clients should call JoinBox after connect
            return base.OnConnectedAsync();
        }

        public Task JoinBox(int chatBoxId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, $"box-{chatBoxId}");
        }

        public Task LeaveBox(int chatBoxId)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"box-{chatBoxId}");
        }

        // Called by client when sending via API or directly via Hub
        public async Task SendMessageToGroup(ChatMessageDTO msg)
        {
            await Clients.Group($"box-{msg.ChatBoxId}")
                         .SendAsync("ReceiveMessage", msg);
        }

        // Typing indicator
        public async Task Typing(int chatBoxId)
        {
            await Clients.GroupExcept($"box-{chatBoxId}", Context.ConnectionId)
                         .SendAsync("Typing", chatBoxId);
        }
    }
}

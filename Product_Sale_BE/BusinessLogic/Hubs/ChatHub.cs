using DataAccess.DTOs.ChatDTOs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BusinessLogic.Hubs
{
    public class ChatHub : Hub
    {
        public Task JoinBox(string boxId) =>
            Groups.AddToGroupAsync(Context.ConnectionId, boxId);

        public async Task BroadcastMessage(ChatMessageDTO message)
        {
            await Clients
                .Group(message.ChatBoxId.ToString())
                .SendAsync("ReceiveMessage", message);
        }
    }
}

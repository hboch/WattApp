using WatApps.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WatApps.Hubs
{
    public class ChatHub : Hub
    {
        static List<User> ConnectedUsers = new List<User>();

        public async Task MessageSend(string userId, string userName, string message)
        {
            await Clients.All.SendAsync("onMessageReceive", userId, userName, message);
        }

        public async Task MessageTyping(string userId, string userName)
        {
            await Clients.All.SendAsync("onMessageTyping", userId, userName);
        }

        public async Task Connect()
        {
            var userId = Context.UserIdentifier;
            var id = Context.ConnectionId;
            var userName = Context.User.Identity.Name;

            if (ConnectedUsers.Count(x => x.UserId == userId) == 0)
            {
                ConnectedUsers.Add(new User() { UserId = userId, Name = userName, ConnectionId = id });

                //await Groups.AddToGroupAsync(id, language);
                await Clients.Caller.SendAsync("onConnected", ConnectedUsers, userName, userId);
                await Clients.AllExcept(id).SendAsync("onConnectedNewUser", userName, userId);
            }

        }
        public async Task Disconnect()
        {
            var user = ConnectedUsers.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            if (user != null)
            {
                ConnectedUsers.Remove(user);
                await Clients.AllExcept(user.ConnectionId).SendAsync("onDisconnected", user.UserId);
            }
        }
    }
}

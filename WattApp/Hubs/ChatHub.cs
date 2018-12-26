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
        const string cGroupName = "Users";

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
        //    var userId = Context.UserIdentifier;
        //    var connectionId = GetConnectionId();
        //    var userName = Context.User.Identity.Name;

        //    if (ConnectedUsers.Count(x => x.UserId == userId) == 0)
        //    {
        //        ConnectedUsers.Add(new User() { UserId = userId, Name = userName, ConnectionId = connectionId });
        //    }
        //    //await Groups.AddToGroupAsync(id, language);
        //    await Clients.Caller.SendAsync("onConnected", ConnectedUsers, userId, userName);
        //    await Clients.AllExcept(connectionId).SendAsync("onConnectedNewUser", userId, userName);
        }
               

        public override async Task OnConnectedAsync()
        {
            User currentUser = GetCurrentUser();

            Console.WriteLine("Client connected - Client-Id: {0}", currentUser.ConnectionId);

            //Add new user to the Users group
            await Groups.AddToGroupAsync(currentUser.ConnectionId, cGroupName);

            //Add new user to the list of connected users
            if (ConnectedUsers.Count(x => x.ConnectionId == currentUser.ConnectionId) == 0)
            {
                ConnectedUsers.Add(currentUser);
            }
                        
            await Clients.User(currentUser.UserId).SendAsync("onConnectedThisUser", currentUser.UserId, currentUser.Name);
            await Clients.Group(cGroupName).SendAsync("onConnected", ConnectedUsers);
        
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            User currentUser = GetCurrentUser();

            await Groups.RemoveFromGroupAsync(currentUser.ConnectionId, cGroupName);

            //Remove user from the list of connected users
            if (ConnectedUsers.Count(x => x.ConnectionId == currentUser.ConnectionId) > 0)
            {
                var userToDelete = ConnectedUsers.First( x => x.ConnectionId == currentUser.ConnectionId);
                ConnectedUsers.Remove(userToDelete);
            }

            await Clients.Group(cGroupName).SendAsync("onDisconnected", currentUser.UserId);
            Console.WriteLine("Client disconnected - Client-Id: {0}", currentUser.ConnectionId);

            //Console.WriteLine("Disconnection due to: {0}", exception);
            await base.OnDisconnectedAsync(exception);
        }

        private User GetCurrentUser()
        {
            var connectionId = GetConnectionId();
            var userId = GetUserId();
            var userName = GetUserName();
            var user = new User() { UserId = userId, ConnectionId = connectionId, Name = userName };

            return user;
        }

        private string GetConnectionId()
        {
            return Context.ConnectionId;
        }
        private string GetUserId()
        {
            return Context.UserIdentifier;
        }
        private string GetUserName()
        {
            return Context.User.Identity.Name;
        }
    }
}

using Microsoft.AspNetCore.SignalR;

namespace AllYourPlates.Hubs
{
    public class PlateHub : Hub
    {
        public async Task SendUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}

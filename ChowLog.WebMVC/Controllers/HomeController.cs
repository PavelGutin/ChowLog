using ChowLog.Hubs;
using ChowLog.WebMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChowLog.WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private readonly IHubContext<NotificationHub> _hubContext;

        public HomeController(ILogger<HomeController> logger/*, IHubContext<NotificationHub> hubContext*/)
        {
            _logger = logger;
            //_hubContext = hubContext;
        }

        public IActionResult Index()
        {
            //NotifyClients("Hello from PLATES INDEX!" + DateTime.UtcNow.ToLongTimeString());
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task NotifyClients(string message)
        {
            // Notify all clients connected to the NotificationHub
            //await _hubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}

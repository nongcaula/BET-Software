using Microsoft.AspNetCore.Mvc;

namespace BET.Controllers
{
    public class OrderController : Controller
    {
        private readonly ILogger<OrderController> _logger;
        string baseurl = "https://localhost:7200/";
        public OrderController(ILogger<OrderController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}

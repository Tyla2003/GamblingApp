using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers
{
    /// <summary>
    /// Serves the Blackjack game UI.  
    /// This controller is separate from the Blackjack API logic  
    /// it only returns the Razor view where the JavaScript frontend
    /// interacts with the REST API.
    /// </summary>
    public class BlackJackViewsController : Controller
    {
        [HttpGet("/Blackjack")]
        [HttpGet("/Blackjack/Index")]
        public IActionResult Index()
        {
            // Explicit path so it uses Views/Blackjack/Index.cshtml
            return View("~/Views/Blackjack/Index.cshtml");
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers
{
    /// <summary>
    /// Serves the Slots UI page.  
    /// All game logic is handled in the SlotsController API 
    /// this controller simply loads the UI that runs the JavaScript front end.
    /// </summary>
    public class SlotsViewController : Controller
    {
        // GET /Slots 
        [HttpGet("/Slots")]
        [HttpGet("/Slots/Index")]
        public IActionResult Index()
        {
            // Explicit path so it uses Views/Slots/Index.cshtml
            return View("~/Views/Slots/Index.cshtml");
        }
    }
}
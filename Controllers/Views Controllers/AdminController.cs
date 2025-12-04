using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers
{
    /// <summary>
    /// Loads the Admin Dashboard view.  
    /// Actual admin actions are handled
    /// through the API this controller only displays the page.
    /// </summary>
    public class AdminController : Controller
    {
        // GET: /Admin
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
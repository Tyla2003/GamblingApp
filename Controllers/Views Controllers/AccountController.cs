using Microsoft.AspNetCore.Mvc;

namespace GamblingApp.Controllers
{

    /// <summary>
    /// Handles the Razor Page views for login, registration,
    /// and account management.  
    /// This controller does not contain business logic 
    /// it simply returns the correct pages so the user can
    /// sign in, create an account, or manage their profile.
    /// </summary>
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpGet]
        public IActionResult Manage() => View();
    }
}
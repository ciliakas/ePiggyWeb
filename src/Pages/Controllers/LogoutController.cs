using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ePiggyWeb.Pages.Controllers
{
    public class LogoutController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> LogoutTask()
        {
            Debug.Write("\n\n\n\n\n I'm called Logout!");
            await HttpContext.SignOutAsync();
            return Redirect("/Index");
        }
    }
}

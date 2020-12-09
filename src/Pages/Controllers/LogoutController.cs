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
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("StartDate");
            Response.Cookies.Delete("EndDate");
            return Redirect("/Index");
        }
    }
}

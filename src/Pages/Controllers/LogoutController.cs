using System.Threading.Tasks;
using ePiggyWeb.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ePiggyWeb.Pages.Controllers
{
    public class LogoutController : Controller
    {
        private IMemoryCache Cache { get; }
        
        public LogoutController(IMemoryCache cache)
        {
            Cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> LogoutTask()
        {
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("StartDate");
            Response.Cookies.Delete("EndDate");
            Cache.Remove(CacheKeys.UserCurrency);
            return Redirect("/Index");
        }
    }
}

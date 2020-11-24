using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ePiggyWeb.Pages.Controllers
{
    public class SnowController : Controller
    {
        public IActionResult StartSnow(string returnUrl)
        {
            var option = new CookieOptions() { Expires = DateTime.Now.AddMinutes(30) };
            Response.Cookies.Append("Snow","True", option);
            return Redirect(returnUrl ?? "/index");
        }
        public IActionResult StopSnow(string returnUrl)
        {
            Response.Cookies.Delete("Snow");
            return Redirect(returnUrl ?? "/index");
        }
    }
}

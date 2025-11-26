using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WowAchievementsApp.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string region)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(region))
            {
                // If region is not provided, redirect back to home to select one
                return RedirectToAction("Index", "Home");
            }

            // Store the selected region in a cookie for later use when making API calls
            HttpContext.Response.Cookies.Append("region", region, new CookieOptions { Expires = DateTimeOffset.UtcNow.AddHours(1) });

            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginCallback", "Account"),
                Items = { { "region", region } }
            };

            return Challenge(properties, "BattleNet");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult LoginCallback()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogWebsite.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            HttpContext.Session.Remove("AccessToken");
            Response.Cookies.Delete("RefreshToken");
            return RedirectToPage("/Index");
        }
    }
}

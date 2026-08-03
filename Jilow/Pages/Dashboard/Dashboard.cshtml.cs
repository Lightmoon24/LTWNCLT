using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Jilow.Pages.Dashboard;

public class IndexModel : PageModel
{
    public string? Email { get; set; }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login");
        }

        Email = HttpContext.Session.GetString("Email");

        return Page();
    }
}
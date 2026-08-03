using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Jilow.Pages
{
    public class IndexModel : PageModel
    {
        public string? Test { get; set; }

        public void OnGet()
        {
            HttpContext.Session.SetString("Test", "Hello");
            Test = HttpContext.Session.GetString("Test");
        }
    }
}
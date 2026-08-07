using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Supabase;

namespace Jilow.Pages.Profile
{
    public class Profile : PageModel
    {
        private readonly ILogger<Profile> _logger;
        private readonly Client _supabase;

        public Profile(ILogger<Profile> logger, Client supabase)
        {
            _logger = logger;
            _supabase = supabase;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // Đăng xuất Supabase
            await _supabase.Auth.SignOut();

            // Xóa Session
            HttpContext.Session.Clear();

            // Quay về trang Login
            return RedirectToPage("/Index");
        }
      
    }
}
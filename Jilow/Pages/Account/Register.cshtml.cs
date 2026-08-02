using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase;

namespace Jilow.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly Client _supabase;

    public RegisterModel(Client supabase)
    {
        _supabase = supabase;
    }

    [BindProperty]
    public string Email { get; set; } = "";

    [BindProperty]
    public string Password { get; set; } = "";

    [BindProperty]
    public string ConfirmPassword { get; set; } = "";

    public string ErrorMessage { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Mật khẩu xác nhận không khớp.";
            return Page();
        }

        try
        {
            await _supabase.Auth.SignUp(
                email: Email,
                password: Password
            );

            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
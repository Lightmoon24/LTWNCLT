using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Supabase;

public class LoginModel : PageModel
{
    private readonly Client _supabase;

    public LoginModel(Client supabase)
    {
        _supabase = supabase;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var session = await _supabase.Auth.SignInWithPassword(
                email: Email,
                password: Password
            );

            if (session?.User != null)
            {
                return RedirectToPage("/Index");
            }

            ErrorMessage = "Email hoặc mật khẩu không đúng.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
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
        // Kiểm tra email
        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Vui lòng nhập email.";
            return Page();
        }

        // Kiểm tra mật khẩu
        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Vui lòng nhập mật khẩu.";
            return Page();
        }

        // Kiểm tra xác nhận mật khẩu
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Mật khẩu xác nhận không khớp.";
            return Page();
        }

        try
        {
            await _supabase.Auth.SignUp(
                email: Email.Trim(),
                password: Password
            );

            // Đăng ký thành công
            TempData["SuccessMessage"] = "Bạn đã đăng ký thành công";

            // Chuyển về trang Login
            return RedirectToPage("/Account/Login");
        }
        catch (Exception ex)
        {
            // Supabase thường trả về message liên quan đến
            // "already registered" khi email đã tồn tại.
            string error = ex.Message.ToLowerInvariant();

            if (error.Contains("already registered") ||
                error.Contains("already exists") ||
                error.Contains("user already exists"))
            {
                ErrorMessage = "Email đăng ký này đã tồn tại";
            }
            else
            {
                ErrorMessage = "Đăng ký không thành công. Vui lòng kiểm tra thông tin và thử lại.";
            }

            return Page();
        }
    }
}
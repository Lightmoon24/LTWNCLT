using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Jilow.Data;
using Supabase;

namespace Jilow.Pages.Profile;

public class Profile : PageModel
{
    private readonly ILogger<Profile> _logger;

    // Ghi rõ namespace để tránh Client bị ambiguous
    private readonly Supabase.Client _supabase;

    private readonly AppDbContext _context;

    // Tên bucket trong Supabase Storage
    private const string AvatarBucket = "Avatar_img";

    public Profile(
        ILogger<Profile> logger,
        Supabase.Client supabase,
        AppDbContext context)
    {
        _logger = logger;
        _supabase = supabase;
        _context = context;
    }


    // ===========================
    // PROFILE
    // ===========================

    [BindProperty]
    public Models.Profile UserProfile { get; set; } = default!;


    // ===========================
    // AVATAR
    // ===========================

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }


    // ===========================
    // MESSAGE
    // ===========================

    public string? ErrorMessage { get; set; }

    public string? SuccessMessage { get; set; }


    // ===========================
    // GET
    // ===========================

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!Guid.TryParse(userId, out var profileId))
        {
            return RedirectToPage("/Account/Login");
        }

        UserProfile = await _context.Profiles
            .FirstOrDefaultAsync(x => x.Id == profileId);

        if (UserProfile == null)
        {
            return NotFound();
        }

        return Page();
    }


    // ===========================
    // UPDATE PROFILE
    // ===========================

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = HttpContext.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!Guid.TryParse(userId, out var profileId))
        {
            return RedirectToPage("/Account/Login");
        }

        var profile = await _context.Profiles
            .FirstOrDefaultAsync(x => x.Id == profileId);

        if (profile == null)
        {
            return NotFound();
        }


        // ===========================
        // UPDATE PROFILE INFORMATION
        // ===========================

        profile.FullName = UserProfile.FullName;
        profile.Username = UserProfile.Username;
        profile.Gender = UserProfile.Gender;
        profile.Country = UserProfile.Country;
        profile.Language = UserProfile.Language;
        profile.Timezone = UserProfile.Timezone;


        // ===========================
        // UPLOAD AVATAR
        // ===========================

        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            var extension = Path
                .GetExtension(AvatarFile.FileName)
                .ToLowerInvariant();


            // Kiểm tra định dạng
            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage =
                    "Chỉ cho phép file ảnh JPG, JPEG, PNG, GIF hoặc WEBP.";

                UserProfile = profile;

                return Page();
            }


            // Giới hạn 5MB
            const long maxFileSize = 5 * 1024 * 1024;

            if (AvatarFile.Length > maxFileSize)
            {
                ErrorMessage =
                    "Ảnh đại diện không được vượt quá 5MB.";

                UserProfile = profile;

                return Page();
            }


            try
            {
                // ===========================
                // STORAGE PATH
                // ===========================

                var avatarPath =
                    $"{profile.Id}/avatar{extension}";


                // ===========================
                // READ FILE
                // ===========================

                await using var memoryStream =
                    new MemoryStream();

                await AvatarFile.CopyToAsync(memoryStream);

                var fileBytes =
                    memoryStream.ToArray();


                // ===========================
                // UPLOAD TO SUPABASE
                // ===========================

                await _supabase.Storage
                    .From(AvatarBucket)
                    .Upload(
                        fileBytes,
                        avatarPath,
                        new Supabase.Storage.FileOptions
                        {
                            CacheControl = "3600",
                            Upsert = true
                        }
                    );


                // ===========================
                // GET PUBLIC URL
                // ===========================

                var avatarUrl = _supabase.Storage
                    .From(AvatarBucket)
                    .GetPublicUrl(avatarPath);


                // ===========================
                // SAVE URL
                // ===========================

                profile.AvatarUrl = avatarUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Không thể upload avatar cho user {UserId}",
                    profile.Id
                );

                ErrorMessage =
                    "Không thể tải ảnh đại diện lên. Vui lòng thử lại.";

                UserProfile = profile;

                return Page();
            }
        }


        // ===========================
        // SAVE DATABASE
        // ===========================

        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();


        SuccessMessage =
            "Cập nhật thông tin thành công.";

        return RedirectToPage();
    }


    // ===========================
    // LOGOUT
    // ===========================

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await _supabase.Auth.SignOut();

        HttpContext.Session.Clear();

        return RedirectToPage("/Index");
    }
}
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
    private readonly Supabase.Client _supabase;
    private readonly AppDbContext _context;

    /*
     * QUAN TRỌNG:
     *
     * Tên này PHẢI giống 100% tên bucket trong
     * Supabase Dashboard -> Storage.
     *
     * Ví dụ nếu bucket của bạn tên:
     * Avatar_img
     *
     * thì giữ nguyên.
     */
    private const string AvatarBucket = "Avatar_img";


    // ===========================
    // CONSTRUCTOR
    // ===========================

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

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!Guid.TryParse(userId, out var profileId))
        {
            return RedirectToPage("/Account/Login");
        }


        UserProfile = await _context.Profiles
            .AsNoTracking()
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

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!Guid.TryParse(userId, out var profileId))
        {
            return RedirectToPage("/Account/Login");
        }


        // ===========================
        // GET PROFILE FROM DATABASE
        // ===========================

        var profile = await _context.Profiles
            .FirstOrDefaultAsync(x => x.Id == profileId);


        if (profile == null)
        {
            return NotFound();
        }


        // ===========================
        // UPDATE PROFILE INFORMATION
        // ===========================

        if (UserProfile != null)
        {
            profile.FullName = UserProfile.FullName;
            profile.Username = UserProfile.Username;
            profile.Gender = UserProfile.Gender;
            profile.Country = UserProfile.Country;
            profile.Language = UserProfile.Language;
            profile.Timezone = UserProfile.Timezone;
        }


        // ===========================
        // UPLOAD AVATAR
        // ===========================

        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            var extension = Path
                .GetExtension(AvatarFile.FileName)
                .ToLowerInvariant();


            // ===========================
            // CHECK EXTENSION
            // ===========================

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };


            if (!allowedExtensions.Contains(extension))
            {
                ErrorMessage =
                    "Chỉ cho phép file ảnh JPG, JPEG, PNG, GIF hoặc WEBP.";

                UserProfile = profile;

                return Page();
            }


            // ===========================
            // CHECK FILE SIZE
            // ===========================

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
                // FILE PATH
                // ===========================

                /*
                 * Ví dụ:
                 *
                 * 550e8400-e29b-41d4-a716-446655440000/avatar.png
                 *
                 * Mỗi user có một folder riêng.
                 */

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
                // UPLOAD TO SUPABASE STORAGE
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

                /*
                 * URL được lấy SAU KHI upload thành công.
                 */

                var avatarUrl = _supabase.Storage
                    .From(AvatarBucket)
                    .GetPublicUrl(avatarPath);


                if (string.IsNullOrWhiteSpace(avatarUrl))
                {
                    throw new Exception(
                        "Supabase không trả về URL của avatar."
                    );
                }


                // ===========================
                // SAVE URL TO DATABASE
                // ===========================

                profile.AvatarUrl = avatarUrl;


                _logger.LogInformation(
                    "Avatar uploaded successfully. UserId: {UserId}, Url: {AvatarUrl}",
                    profile.Id,
                    avatarUrl
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Không thể upload avatar cho user {UserId}",
                    profile.Id
                );


                /*
                 * Hiển thị lỗi thực tế trong Development
                 * để dễ debug Supabase.
                 */

                if (HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment())
                {
                    ErrorMessage =
                        $"Upload avatar thất bại: {ex.Message}";
                }
                else
                {
                    ErrorMessage =
                        "Không thể tải ảnh đại diện lên. Vui lòng thử lại.";
                }


                UserProfile = profile;

                return Page();
            }
        }


        // ===========================
        // UPDATE TIME
        // ===========================

        profile.UpdatedAt = DateTime.UtcNow;


        // ===========================
        // SAVE DATABASE
        // ===========================

        await _context.SaveChangesAsync();


        // ===========================
        // SUCCESS
        // ===========================

        TempData["SuccessMessage"] =
            "Cập nhật thông tin thành công.";


        return RedirectToPage();
    }


    // ===========================
    // LOGOUT
    // ===========================

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        try
        {
            await _supabase.Auth.SignOut();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Supabase logout failed."
            );
        }


        HttpContext.Session.Clear();


        return RedirectToPage("/Index");
    }
}

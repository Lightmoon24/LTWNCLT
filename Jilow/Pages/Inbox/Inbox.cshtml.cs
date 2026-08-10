using Jilow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Jilow.Pages.Inbox;

public class InboxModel : PageModel
{
    private readonly Supabase.Client _supabase;

    public InboxModel(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    [BindProperty]
    public InboxMessageInput Input { get; set; } = new();

    public List<Message> Messages { get; set; } = new();

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }

    public string? ActiveEmail { get; set; }

    public async Task OnGetAsync(string? email = null)
    {
        ActiveEmail = email;
        await LoadMessagesAsync(email);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.Title) ||
            string.IsNullOrWhiteSpace(Input.SenderEmail) ||
            string.IsNullOrWhiteSpace(Input.RecipientEmail) ||
            string.IsNullOrWhiteSpace(Input.Content))
        {
            ErrorMessage = "Vui lòng điền đầy đủ Tiêu đề, Email người gửi, Email người nhận và Nội dung.";
            await LoadMessagesAsync();
            return Page();
        }

        try
        {
            var message = new Message
            {
                Title = Input.Title.Trim(),
                SenderEmail = Input.SenderEmail.Trim(),
                RecipientEmail = Input.RecipientEmail.Trim(),
                Content = Input.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _supabase
                .From<Message>()
                .Insert(message);

            SuccessMessage = "Tin nhắn đã được lưu và gửi tới người dùng nhận.";
            Input = new InboxMessageInput();
            await LoadMessagesAsync();
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Không thể lưu tin nhắn: " + ex.Message;
            await LoadMessagesAsync();
            return Page();
        }
    }

    private async Task LoadMessagesAsync(string? email = null)
    {
        try
        {
            var response = await _supabase
                .From<Message>()
                .Get();

            var allMessages = response.Models
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            if (!string.IsNullOrWhiteSpace(email))
            {
                Messages = allMessages
                    .Where(x =>
                        x.SenderEmail.Equals(email, StringComparison.OrdinalIgnoreCase) ||
                        x.RecipientEmail.Equals(email, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                Messages = allMessages
                    .Take(50)
                    .ToList();
            }
        }
        catch
        {
            Messages = new List<Message>();
        }
    }
}

public class InboxMessageInput
{
    public string Title { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string RecipientEmail { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}

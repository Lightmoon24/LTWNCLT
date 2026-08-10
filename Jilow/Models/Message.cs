using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Jilow.Models;

[Table("messages")]
public class Message : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("sender_email")]
    public string SenderEmail { get; set; } = string.Empty;

    [Column("recipient_email")]
    public string RecipientEmail { get; set; } = string.Empty;

    [Column("content")]
    public string Content { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

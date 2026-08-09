using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Jilow.Models;

[Table("boards")]
public class Board : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "My Board";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
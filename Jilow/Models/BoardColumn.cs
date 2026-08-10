using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Jilow.Models;

[Table("board_columns")]
public class BoardColumn : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }


    [Column("board_id")]
    public long BoardId { get; set; }


    [Column("name")]
    public string Name { get; set; }
        = string.Empty;


    [Column("position")]
    public int Position { get; set; }


    [Column("color_class")]
    public string? ColorClass { get; set; }


    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Jilow.Models;

[Table("tickets")]
public class Ticket : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }


    [Column("column_id")]
    public long ColumnId { get; set; }


    [Column("ticket_key")]
    public string Key { get; set; }
        = string.Empty;


    [Column("title")]
    public string Title { get; set; }
        = string.Empty;


    [Column("description")]
    public string? Description { get; set; }


    [Column("start_date")]
    public DateTime? StartDate { get; set; }


    [Column("end_date")]
    public DateTime? EndDate { get; set; }


    [Column("priority")]
    public string Priority { get; set; }
        = "Medium";


    [Column("position")]
    public int Position { get; set; }


    [Column("created_at")]
    public DateTime CreatedAt { get; set; }


    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
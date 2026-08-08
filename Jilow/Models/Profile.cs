using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jilow.Models;

[Table("profiles")]
public class Profile
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("full_name")]
    public string? FullName { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("birthday")]
    public DateOnly? Birthday { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("language")]
    public string? Language { get; set; }

    [Column("timezone")]
    public string? Timezone { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("bio")]
    public string? Bio { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
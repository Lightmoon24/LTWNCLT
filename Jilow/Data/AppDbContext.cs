using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Jilow.Models;

[Table("profiles")]
public class Profile
{
    [Key]
    public Guid Id { get; set; }

    public string? Username { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Gender { get; set; }

    public string? Country { get; set; }

    public string? Language { get; set; }

    public string? Timezone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
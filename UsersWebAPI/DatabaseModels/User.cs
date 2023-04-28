using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace UsersWebAPI.DatabaseModels;

[Index(nameof(Login), IsUnique = true)]
public class User
{
    [Key] public Guid Guid { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string Login { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string Password { get; set; }

    [Required, RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ]+$")]
    public string Name { get; set; }

    [Required, RegularExpression(@"[012]")]
    public int Gender { get; set; }

    public DateTime? Birthday { get; set; }

    [Required] public bool Admin { get; set; }

    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }

    public DateTime ModifiedOn { get; set; }
    public string ModifiedBy { get; set; }

    public DateTime? RevokedOn { get; set; }
    public string? RevokedBy { get; set; }
}
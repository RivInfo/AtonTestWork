using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class CreateUser : IAuthData
{
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string AuthLogin { get; set; }
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string AuthPassword { get; set; }
    
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string Login { get; set; }
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string Password { get; set; }

    [Required, RegularExpression(@"^[a-zA-Zа-яА-ЯёЁ]+$")] public string Name { get; set; }

    public DateTime? Birthday { get; set; }
    
    [Required, RegularExpression(@"[012]")] public int Gender { get; set; }
    
    [Required] public bool IsAdmin { get; set; }
}
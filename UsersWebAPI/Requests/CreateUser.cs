using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class CreateUser : IAuthData
{
    [Required] public string AuthLogin { get; set; }
    [Required] public string AuthPassword { get; set; }
    
    [Required] public string Login { get; set; }
    [Required] public string Password { get; set; }

    [Required] public string Name { get; set; }

    public DateTime? Birthday { get; set; }
    
    [Required] public int Gender { get; set; }
    
    public bool? IsAdmin { get; set; }
}
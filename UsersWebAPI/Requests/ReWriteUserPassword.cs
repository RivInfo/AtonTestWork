using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class ReWriteUserPassword : IAuthData
{
    [Required] public string AuthLogin { get; set; }
    [Required] public string AuthPassword { get; set; }
    
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string ReLogin { get; set; }
    
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")] public string NewPassword { get; set; }
}
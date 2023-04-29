using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class ReWriteUserLogin : IAuthData
{
    [Required] public string AuthLogin { get; set; }
    [Required] public string AuthPassword { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string CurrentLogin { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string NewLogin { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class RevokeDeleteRequest : IAuthData
{
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string AuthLogin { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string AuthPassword { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string Login { get; set; }
}
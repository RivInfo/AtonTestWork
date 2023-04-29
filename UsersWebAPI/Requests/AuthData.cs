using System.ComponentModel.DataAnnotations;

namespace UsersWebAPI.Requests;

public class AuthData : IAuthData
{
    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string AuthLogin { get; set; }

    [Required, RegularExpression(@"^[a-zA-Z0-9]+$")]
    public string AuthPassword { get; set; }

    public AuthData(string login, string password)
    {
        AuthLogin = login;
        AuthPassword = password;
    }
}
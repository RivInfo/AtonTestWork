namespace UsersWebAPI.Requests;

public interface IAuthData
{
    public string AuthLogin { get; set; }
    public string AuthPassword { get; set; }
}
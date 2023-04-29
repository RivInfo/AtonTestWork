using UsersWebAPI.DatabaseModels;
using UsersWebAPI.Requests;

namespace UsersWebAPI.Responses;

public class UserResponse
{
    public string Name { get; set; }
    public int Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public bool IsActive { get; set; }

    public UserResponse(User user)
    {
        Name = user.Name;
        Gender = user.Gender;
        Birthday = user.Birthday;
        IsActive = user.RevokedOn == null;
    }
}
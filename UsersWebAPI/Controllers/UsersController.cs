using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;
using UsersWebAPI.DatabaseModels;
using UsersWebAPI.DB;
using UsersWebAPI.Options;
using UsersWebAPI.Requests;
using UsersWebAPI.Responses;

namespace UsersWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    private readonly UsersContext _usersContext;

    public UsersController(ILogger<UsersController> logger, UsersContext usersContext)
    {
        _logger = logger;
        _usersContext = usersContext;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(CreateUser createData)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User? userAuth = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == createData.AuthLogin);

        if (!UserAuthentication(createData, userAuth))
            return BadRequest();

        User? dublicate = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == createData.Login);

        if (dublicate != null)
            return Conflict();

        User newUser = new User()
        {
            Login = createData.Login,
            Name = createData.Name,
            Password = createData.Password,
            Gender = createData.Gender,
            Birthday = createData.Birthday,
            CreatedBy = createData.AuthLogin,
            CreatedOn = DateTime.UtcNow,
            ModifiedOn = DateTime.UtcNow,
            ModifiedBy = createData.AuthLogin
        };

        // if(!TryValidateModel(newUser)) //Validate variant 2
        //     return BadRequest();

        _usersContext.Users.Add(newUser);

        await _usersContext.SaveChangesAsync();

        return Accepted();
    }

    [HttpPut("rewriteNGB")]
    public async Task<ActionResult<string>> ReWriteNGB(ReWriteUserNGB reWriteUserData)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        if (reWriteUserData.Name == null && reWriteUserData.Gender == null && reWriteUserData.Birthday == null)
            return BadRequest();
            
        User? userAuth = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == reWriteUserData.AuthLogin);

        if (!UserAuthentication(reWriteUserData, userAuth))
            return BadRequest();

        User? reUser = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == reWriteUserData.ReLogin);

        if (reUser == null)
            return BadRequest();

        if (!userAuth.Admin && reUser.Login != reWriteUserData.ReLogin)
            return BadRequest();

        if (reWriteUserData.Name != null)
            reUser.Name = reWriteUserData.Name;

        if (reWriteUserData.Gender != null)
            reUser.Gender = (int)reWriteUserData.Gender;

        if (reWriteUserData.Birthday != null)
            reUser.Birthday = reWriteUserData.Birthday;

        SetModified(reUser, userAuth);

        await _usersContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("rewritePassword")]
    public async Task<ActionResult<string>> ReWritePassword(ReWriteUserPassword reWriteUserPassword)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserPassword.AuthLogin);

        if (!UserAuthentication(reWriteUserPassword, userAuth))
            return BadRequest();

        User? reUser = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserPassword.ReLogin);

        if (reUser == null)
            return BadRequest();

        if (!userAuth.Admin && reUser.Login != reWriteUserPassword.ReLogin)
            return BadRequest();

        reUser.Password = reWriteUserPassword.NewPassword;
        
        SetModified(reUser, userAuth);

        await _usersContext.SaveChangesAsync();

        return Ok();
    }

    //жесть, но в базу данных (Postgresql) не получилось Guid как ключ сделать
    [HttpPut("rewriteLogin")]
    public async Task<ActionResult<string>> ReWriteLogin(ReWriteUserLogin reWriteUserLogin)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserLogin.AuthLogin);

        if (!UserAuthentication(reWriteUserLogin, userAuth))
            return BadRequest();

        User? reUser = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserLogin.CurrentLogin);

        if (reUser == null)
            return BadRequest();

        if (!userAuth.Admin && reUser.Login != reWriteUserLogin.CurrentLogin)
            return BadRequest();

        User? dublicate = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserLogin.NewLogin);

        if (dublicate != null)
            return Conflict();

        var newUser = await _usersContext.Users.AddAsync(new User
        {
            Login = reWriteUserLogin.NewLogin,
            Password = reUser.Password,
            Name = reUser.Name,
            Gender = reUser.Gender,
            Birthday = reUser.Birthday,
            Admin = reUser.Admin,
            CreatedOn = reUser.CreatedOn,
            CreatedBy = reUser.CreatedBy,
            ModifiedOn = reUser.ModifiedOn,
            ModifiedBy = reUser.ModifiedBy,
            RevokedOn = reUser.RevokedOn,
            RevokedBy = reUser.RevokedBy
        });

        Guid saveGuid = reUser.Guid;

        await _usersContext.SaveChangesAsync();

        _usersContext.Users.Remove(reUser);

        await _usersContext.SaveChangesAsync();

        newUser.Entity.Guid = saveGuid;
        
        SetModified(newUser.Entity, userAuth);

        await _usersContext.SaveChangesAsync();

        return Ok();
    }


    [HttpGet("getAllActiveUser")]
    public async Task<ActionResult<List<User>>> GetAllActiveUser(string login, string password)
    {
        AuthData authData = new AuthData(login,password);

        if (!TryValidateModel(authData))
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == authData.AuthLogin);

        if (!UserAuthenticationAdmin(authData, userAuth))
            return BadRequest();

        List<User> users = await _usersContext.Users.Where(x => x.RevokedOn == null)
            .OrderBy(x => x.CreatedOn).ToListAsync();

        return Ok(users);
    }

    [HttpGet("getUserInfo")]
    public async Task<ActionResult<List<UserResponse>>> GetUserInfo(string login, string password, string selectLogin)
    {
        AuthData authData = new AuthData(login,password);

        if (!TryValidateModel(authData))
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == authData.AuthLogin);

        if (!UserAuthenticationAdmin(authData, userAuth))
            return BadRequest();

        User? user = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == selectLogin);

        UserResponse? userResponse = null;

        if (user != null)
            userResponse = new UserResponse(user);

        return Ok(userResponse);
    }

    [HttpGet("getUser")]
    public async Task<ActionResult<User>> GetUser(string login, string password)
    {
        AuthData authData = new AuthData(login,password);

        if (!TryValidateModel(authData))
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == authData.AuthLogin);

        if (!UserAuthentication(authData, userAuth))
            return BadRequest();

        return Ok(userAuth);
    }

    [HttpGet("getAllUserByBirthday")]
    public async Task<ActionResult<List<User>>> GetAllUserByBirthday(string login, string password, int year)
    {
        AuthData authData = new AuthData(login,password);

        if (!TryValidateModel(authData))
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == authData.AuthLogin);

        if (!UserAuthenticationAdmin(authData, userAuth))
            return BadRequest();

        List<User> users = await _usersContext.Users
            .Where(x => x.Birthday != null).ToListAsync();

        users = users.Where(x => ResultAge((DateTime)x.Birthday) > year).ToList();

        return Ok(users);
    }

    [HttpDelete("deleteUser")]
    public async Task<ActionResult<string>> DeleteUser(string login, string password, 
        string targetLogin, bool hardDelete)
    {
        AuthData authData = new AuthData(login,password);

        if (!TryValidateModel(authData))
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == authData.AuthLogin);

        if (!UserAuthenticationAdmin(authData, userAuth))
            return BadRequest();

        User? targetUser = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == targetLogin);

        if (targetUser == null)
            return NotFound();

        if (hardDelete)
        {
            _usersContext.Users.Remove(targetUser);
        }
        else
        {
            targetUser.RevokedOn = DateTime.UtcNow;
            targetUser.RevokedBy = userAuth.Login;
            
            SetModified(targetUser, userAuth);
        }

        await _usersContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("RevokeDelete")]
    public async Task<ActionResult<string>> RevokeDelete(RevokeDeleteRequest revokeDelete)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User? userAuth = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == revokeDelete.AuthLogin);

        if (!UserAuthenticationAdmin(revokeDelete, userAuth))
            return BadRequest();

        User? targetUser = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == revokeDelete.Login);

        if (targetUser == null)
            return NotFound();

        targetUser.RevokedBy = null;
        targetUser.RevokedOn = null;
        
        SetModified(targetUser, userAuth);
        
        await _usersContext.SaveChangesAsync();

        return Ok();
    }
    

    private static bool UserAuthentication(IAuthData user, User? authUser)
    {
        if (authUser == null)
            return false;

        if (authUser.RevokedOn != null)
            return false;

        if (authUser.Password != user.AuthPassword)
            return false;

        return true;
    }

    private static bool UserAuthenticationAdmin(IAuthData user, User? authUser)
    {
        if (!UserAuthentication(user, authUser))
            return false;

        if (!authUser.Admin)
            return false;

        return true;
    }

    private static void SetModified(User modifiable, User modifying)
    {
        modifiable.ModifiedBy = modifying.Login;
        modifiable.ModifiedOn = DateTime.UtcNow;
    }

    private static int ResultAge(DateTime birthday)
    {
        DateTime today = DateTime.Today;
        int age = today.Year - birthday.Year;
        if (birthday.Date > today.AddYears(-age))
            age--;
        return age;
    }
}
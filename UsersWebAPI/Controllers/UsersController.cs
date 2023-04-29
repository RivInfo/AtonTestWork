using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UsersWebAPI.DatabaseModels;
using UsersWebAPI.DB;
using UsersWebAPI.Options;
using UsersWebAPI.Requests;

namespace UsersWebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;

    private readonly UsersContext _usersContext;

    public UsersController(ILogger<UsersController> logger,
        UsersContext usersContext)
    {
        _logger = logger;
        _usersContext = usersContext;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(CreateUser createData)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User? user = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == createData.AuthLogin);

        if (user == null)
            return BadRequest();

        if (user.Password != createData.AuthPassword)
            return Unauthorized();

        if (createData.IsAdmin != null)
            if (!user.Admin)
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
            CreatedOn = DateTime.Now.ToUniversalTime(),
            ModifiedOn = DateTime.Now.ToUniversalTime(),
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

        User? userAuth = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == reWriteUserData.AuthLogin);

        if (userAuth == null)
            return BadRequest();

        if (userAuth.RevokedOn != null)
            return Unauthorized();

        if (userAuth.Password != reWriteUserData.AuthPassword)
            return Unauthorized();


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

        if (userAuth == null)
            return BadRequest();

        if (userAuth.RevokedOn != null)
            return Unauthorized();

        if (userAuth.Password != reWriteUserPassword.AuthPassword)
            return Unauthorized();
        
        
        
        User? reUser = await _usersContext.Users
            .FirstOrDefaultAsync(x => x.Login == reWriteUserPassword.ReLogin);

        if (reUser == null)
            return BadRequest();

        if (!userAuth.Admin && reUser.Login != reWriteUserPassword.ReLogin)
            return BadRequest();

        reUser.Password = reWriteUserPassword.NewPassword;

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

        if (userAuth == null)
            return BadRequest();

        if (userAuth.RevokedOn != null)
            return Unauthorized();

        if (userAuth.Password != reWriteUserLogin.AuthPassword)
            return Unauthorized();
        
        
        
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
        
        await _usersContext.SaveChangesAsync();
        
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> GetAll()
    {
        List<User> users = await _usersContext.Users.ToListAsync();

        return Ok(users);
    }
}
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
        _logger.LogInformation("Start controller");
        _usersContext = usersContext;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Create(CreateUser createData)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        User user = await _usersContext.Users.FirstOrDefaultAsync(x => x.Login == createData.AuthLogin);

        if (user != null)
        {
            if (user.Password != createData.AuthPassword)
                return Unauthorized();

            if (createData.IsAdmin != null)
                if (!user.Admin)
                    return BadRequest();
        }

        _usersContext.Users.Add(new User()
        {
            //Guid = new Guid(),
            Login = createData.Login,
            Name = createData.Name,
            Password = createData.Password,
            Gender = createData.Gender,
            Birthday = createData.Birthday,
            CreatedBy = createData.AuthLogin,
            CreatedOn = DateTime.Now.ToUniversalTime(),
            ModifiedOn = DateTime.Now.ToUniversalTime(),
            ModifiedBy = createData.AuthLogin
        });

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
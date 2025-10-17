using Microsoft.AspNetCore.Mvc;
using UserService.Contract.Managers;
using UserService.Model.DTO.User;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // api/users
public class UsersController(IUserManager userManager) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register([FromBody] CreateUserDTO userDto, CancellationToken ct)
    {
        var user = await userManager.RegisterAsync(userDto, ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<UserDTO>> Update(Guid id, [FromBody] UpdateUserDTO userDto, CancellationToken ct)
    {
        var user = await userManager.UpdateAsync(userDto, id, ct);
        return Ok(user);
    }
    

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDTO>> Get(Guid id, CancellationToken ct)
    {
        var user = await userManager.GetAsync(id, ct);
        return Ok(user);
    }
    
    [HttpGet("{id:guid}/main")]
    public async Task<ActionResult<ShortUserDTO>> GetMain(Guid id, CancellationToken ct)
    {
        var user = await userManager.GetShortAsync(id, ct);
        return Ok(user);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await userManager.DeleteAsync(id, ct);
        return NoContent();
    }
}
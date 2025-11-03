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
    public async Task<ActionResult<UserDTO>> Update([FromRoute] Guid id, [FromBody] UpdateUserDTO userDto, CancellationToken ct)
    {
        var user = await userManager.UpdateAsync(userDto, id, ct);
        return Ok(user);
    }
    

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserDTO>> Get([FromRoute] Guid id, CancellationToken ct)
    {
        var user = await userManager.GetAsync(id, ct);
        return Ok(user);
    }
    
    [HttpGet("{id:guid}/main")]
    public async Task<ActionResult<ShortUserDTO>> GetMain([FromRoute] Guid id, CancellationToken ct)
    {
        var user = await userManager.GetShortAsync(id, ct);
        return Ok(user);
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedUsersDTO>> GetAll(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await userManager.GetAllAsync(offset, limit, ct);
        return Ok(result);
    }
    
    [HttpGet("main")]
    public async Task<ActionResult<PagedUsersMainDTO>> GetAllMain(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await userManager.GetAllShortAsync(offset, limit, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await userManager.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("search-api")]
    public async Task<ActionResult<object>> Search([FromQuery] string? uid, [FromQuery] string? nickname,
        CancellationToken ct,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(uid) && string.IsNullOrWhiteSpace(nickname))
        {
            return BadRequest("Укажите Uid или Никнейм для поиска");
        }
        if (!string.IsNullOrWhiteSpace(uid))
        {
            var user = await userManager.GetByUidAsync(uid, ct);
            return Ok(user);
        }
        if (!string.IsNullOrWhiteSpace(nickname))
        {
            var users = await userManager.GetByNickNameAsync(nickname, offset, limit, ct);
            return Ok(users);
        }
        return BadRequest("Неподдерживаемый запрос");
    }
}
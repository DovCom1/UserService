using Microsoft.AspNetCore.Mvc;
using UserService.Contract.Managers;
using UserService.Model.DTO.FriendUser;

namespace UserService.Api.Controllers;


[ApiController]
[Route("api/users/{userId:guid}/[controller]")]
public class FriendsController(IFriendManager friendManager) : ControllerBase
{
    [HttpPost("{friendId:guid}")]
    public async Task<ActionResult<FriendUserDTO>> AddFriend(Guid userId, Guid friendId, CancellationToken ct)
    {
        var dto = new CreateFriendUserDTO(userId, friendId);
        var result = await friendManager.SendRequestAsync(dto, ct);
        return CreatedAtAction(nameof(CheckFriendExists), new { userId, friendId }, result);
    }

    [HttpPatch("{friendId:guid}/accept")]
    public async Task<ActionResult<FriendUserDTO>> AcceptRequest(Guid userId, Guid friendId, CancellationToken ct)
    {
        var dto = new UpdateFriendUserDTO(userId, friendId, "Друг");
        var result = await friendManager.AcceptFriendRequestAsync(dto, ct);
        return Ok(result);
    }

    [HttpPatch("{friendId:guid}/reject")]
    public async Task<ActionResult> RejectRequest(Guid userId, Guid friendId, CancellationToken ct)
    {
        var dto = new DeleteFriendUserDTO(userId, friendId);
        await friendManager.RejectFriendRequestAsync(dto, ct);
        return NoContent();
    }

    [HttpDelete("{friendId:guid}")]
    public async Task<ActionResult> DeleteFriend(Guid userId, Guid friendId, CancellationToken ct)
    {
        var dto = new DeleteFriendUserDTO(userId, friendId);
        await friendManager.DeleteAsync(dto, ct);
        return NoContent();
    }
    
    [HttpGet("{friendId:guid}/exists")]
    public async Task<ActionResult> CheckFriendExists(Guid userId, Guid friendId, CancellationToken ct)
    {
        var exists = await friendManager.CheckFriendExists(userId, friendId, ct);
        return Ok(new { exists });
    }
    
    [HttpGet("")]
    public async Task<ActionResult<PagedFriendResponseDTO>> GetFriends(Guid userId,
        CancellationToken ct, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        var result = await friendManager.GetFriendsAsync(userId, offset, limit, ct);
        return Ok(result);
    }
    
    [HttpGet("requests/incoming")]
    public async Task<ActionResult<PagedFriendResponseDTO>> GetIncomingRequests(Guid userId,
        CancellationToken ct, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        var result = await friendManager.GetIncomingRequestsAsync(userId, offset, limit, ct);
        return Ok(result);
    }
    
    [HttpGet("requests/outgoing")]
    public async Task<ActionResult<PagedFriendResponseDTO>> GetOutcomingRequests(Guid userId,
        CancellationToken ct, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        var result = await friendManager.GetOutcomingRequestsAsync(userId, offset, limit, ct);
        return Ok(result);
    }
}
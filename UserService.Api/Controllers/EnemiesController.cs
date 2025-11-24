using Microsoft.AspNetCore.Mvc;
using UserService.Contract.Managers;
using UserService.Model.DTO.EnemyUser;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/[controller]")]
public class EnemiesController(IEnemyManager enemyManager) : ControllerBase
{
    [HttpPost("{enemyId:guid}")]
    public async Task<ActionResult<EnemyUserDTO>> AddEnemy([FromRoute] Guid userId, [FromRoute] Guid enemyId, CancellationToken ct)
    {
        var dto = new CreateEnemyUserDTO(userId, enemyId);
        var result = await enemyManager.AddAsync(dto, ct);
        return CreatedAtAction(nameof(CheckEnemyExists), new { userId, enemyId }, result);
    }
    
    [HttpDelete("{enemyId:guid}")]
    public async Task<ActionResult> DeleteEnemy([FromRoute] Guid userId, [FromRoute] Guid enemyId, CancellationToken ct)
    {
        var dto = new EnemyUserDTO(userId, enemyId);
        await enemyManager.DeleteAsync(dto, ct);
        return NoContent();
    }
    
    [HttpGet("{enemyId:guid}/exists")]
    public async Task<ActionResult> CheckEnemyExists([FromRoute] Guid userId, [FromRoute] Guid enemyId, CancellationToken ct)
    {
        var exists = await enemyManager.IsEnemy(userId, enemyId, ct);
        return Ok(new { exists });
    }

    [HttpGet]
    public async Task<ActionResult<PagedEnemyResponseDTO>> GetEnemies([FromRoute] Guid userId,
        CancellationToken ct, [FromQuery] int offset = 0, [FromQuery] int limit = 10)
    {
        var result = await enemyManager.GetEnemiesAsync(userId, offset, limit, ct);
        return Ok(result);
    }
}
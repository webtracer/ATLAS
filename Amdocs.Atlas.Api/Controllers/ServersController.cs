using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public ServersController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/servers
    /*[HttpGet]
    public async Task<ActionResult<IEnumerable<Server>>> GetServers()
    {
        var servers = await _db.Servers
            .Include(s => s.Environment)
            .Include(s => s.Role)
            .Include(s => s.Owner)
            .Include(s => s.Location)
            .Include(s => s.Customer)
            .Include(s => s.Vcenter)
            .Include(s => s.Project)
            .AsNoTracking()
            .ToListAsync();

        return Ok(servers);
    }

    // GET: api/servers/5
    [HttpGet("{serverId:int}")]
    public async Task<ActionResult<Server>> GetServer(int serverId)
    {
        var server = await _db.Servers
            .Include(s => s.Environment)
            .Include(s => s.Role)
            .Include(s => s.Owner)
            .Include(s => s.Location)
            .Include(s => s.Customer)
            .Include(s => s.Vcenter)
            .Include(s => s.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServerId == serverId);

        if (server == null)
            return NotFound();

        return Ok(server);
    }*/
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Server>>> GetServers()
    {
        var servers = await _db.Servers
            .AsNoTracking()
            .ToListAsync();

        return Ok(servers);
    }

    [HttpGet("{serverId:int}")]
    public async Task<ActionResult<Server>> GetServer(int serverId)
    {
        var server = await _db.Servers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServerId == serverId);

        if (server == null)
            return NotFound();

        return Ok(server);
    }


    /*
    // POST: api/servers
    [HttpPost]
    public async Task<ActionResult<Server>> CreateServer([FromBody] Server server)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Let the DB generate ServerId (identity), and set timestamps server-side
        server.ServerId = 0;
        server.CreatedAt = DateTime.UtcNow;
        server.UpdatedAt = DateTime.UtcNow;

        _db.Servers.Add(server);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetServer),
            new { serverId = server.ServerId },
            server);
    }
    */
    // POST: api/servers
    [HttpPost]
    public async Task<ActionResult<Server>> CreateServer([FromBody] Server server)
    {
        // Minimal guard just to avoid junk
        if (string.IsNullOrWhiteSpace(server.Hostname))
        {
            return BadRequest("Hostname is required.");
        }

        // Let DB generate identity
        server.ServerId = 0;

        // Set timestamps on the server side
        server.CreatedAt = DateTime.UtcNow;
        server.UpdatedAt = DateTime.UtcNow;

        // We are only using FK IDs here; navigation properties can stay null
        _db.Servers.Add(server);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetServer),
            new { serverId = server.ServerId },
            server);
    }


    // PUT: api/servers/5
    [HttpPut("{serverId:int}")]
    public async Task<IActionResult> UpdateServer(int serverId, [FromBody] Server server)
    {
        if (serverId != server.ServerId)
            return BadRequest("Route id and payload id must match.");

        var existing = await _db.Servers
            .Include(s => s.Environment)
            .Include(s => s.Role)
            .Include(s => s.Owner)
            .Include(s => s.Location)
            .Include(s => s.Customer)
            .Include(s => s.Vcenter)
            .Include(s => s.Project)
            .FirstOrDefaultAsync(s => s.ServerId == serverId);

        if (existing == null)
            return NotFound();

        // Update scalar + FK properties; leave CreatedAt alone.
        existing.Hostname          = server.Hostname;
        existing.Fqdn              = server.Fqdn;
        existing.IpAddress         = server.IpAddress;
        existing.SecondaryIp       = server.SecondaryIp;
        existing.OsName            = server.OsName;
        existing.OsFamily          = server.OsFamily;
        existing.OsMajorVersion    = server.OsMajorVersion;
        existing.IsVm              = server.IsVm;
        existing.SourceType        = server.SourceType;
        existing.VmInstanceUuid    = server.VmInstanceUuid;
        existing.VmBiosUuid        = server.VmBiosUuid;
        existing.IsActive          = server.IsActive;
        existing.LifecycleStatus   = server.LifecycleStatus;
        existing.CommissionedAt    = server.CommissionedAt;
        existing.DecommissionedAt  = server.DecommissionedAt;

        existing.EnvironmentId     = server.EnvironmentId;
        existing.RoleId            = server.RoleId;
        existing.OwnerId           = server.OwnerId;
        existing.LocationId        = server.LocationId;
        existing.CustomerId        = server.CustomerId;
        existing.VcenterId         = server.VcenterId;
        existing.ProjectId         = server.ProjectId;

        // Timestamps
        existing.UpdatedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/servers/5
    [HttpDelete("{serverId:int}")]
    public async Task<IActionResult> DeleteServer(int serverId)
    {
        var server = await _db.Servers
            .FirstOrDefaultAsync(s => s.ServerId == serverId);

        if (server == null)
            return NotFound();

        _db.Servers.Remove(server);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

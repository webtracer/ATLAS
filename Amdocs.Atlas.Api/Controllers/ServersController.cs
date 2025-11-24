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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Server>>> GetServers()
    {
        // If you want includes later (Environment, Role, etc.), you can add them here.
        var servers = await _db.Servers
            .AsNoTracking()
            .ToListAsync();

        return Ok(servers);
    }

    // GET: api/servers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Server>> GetServer(int id)
    {
        var server = await _db.Servers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ServerId == id);

        if (server == null)
            return NotFound();

        return Ok(server);
    }

    // POST: api/servers
    // Called from Servers.razor:
    //   await client.PostAsJsonAsync("api/servers", editModel);
    [HttpPost]
    public async Task<ActionResult<Server>> CreateServer([FromBody] Server server)
    {
        if (server == null)
            return BadRequest("Server payload is required.");

        // Ensure EF treats this as a new row
        server.ServerId = 0;

        _db.Servers.Add(server);
        await _db.SaveChangesAsync();

        // Return 201 with Location header
        return CreatedAtAction(
            nameof(GetServer),
            new { id = server.ServerId },
            server
        );
    }

    // PUT: api/servers/5
    // Called from Servers.razor:
    //   await client.PutAsJsonAsync($"api/servers/{editModel.ServerId}", editModel);
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateServer(int id, [FromBody] Server updated)
    {
        if (updated == null)
            return BadRequest("Server payload is required.");

        if (id != updated.ServerId)
            return BadRequest("Route id and payload ServerId do not match.");

        var existing = await _db.Servers
            .FirstOrDefaultAsync(s => s.ServerId == id);

        if (existing == null)
            return NotFound();

        // Copy all scalar values from updated -> existing
        // This avoids having to manually map each property.
        _db.Entry(existing).CurrentValues.SetValues(updated);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/servers/5
    // Called from Servers.razor:
    //   await client.DeleteAsync($"api/servers/{deleteCandidate.ServerId}");
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteServer(int id)
    {
        var server = await _db.Servers
            .FirstOrDefaultAsync(s => s.ServerId == id);

        if (server == null)
            return NotFound();

        _db.Servers.Remove(server);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

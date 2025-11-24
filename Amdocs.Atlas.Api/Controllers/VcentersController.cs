using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VcentersController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public VcentersController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/vcenters
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vcenter>>> GetVcenters()
    {
        var vcenters = await _db.Vcenters
            .AsNoTracking()
            .OrderBy(v => v.IpAddress)
            .ToListAsync();

        return Ok(vcenters);
    }

    // GET: api/vcenters/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Vcenter>> GetVcenter(int id)
    {
        var vcenter = await _db.Vcenters
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VcenterId == id);

        if (vcenter == null)
            return NotFound();

        return Ok(vcenter);
    }

    // POST: api/vcenters
    [HttpPost]
    public async Task<ActionResult<Vcenter>> CreateVcenter([FromBody] Vcenter vcenter)
    {
        if (vcenter == null)
            return BadRequest("Vcenter payload is required.");

        // Ensure EF treats it as new
        vcenter.VcenterId = 0;
        vcenter.CreatedAt = DateTime.UtcNow;
        vcenter.UpdatedAt = DateTime.UtcNow;

        _db.Vcenters.Add(vcenter);
        await _db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetVcenter),
            new { id = vcenter.VcenterId },
            vcenter);
    }

    // PUT: api/vcenters/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVcenter(int id, [FromBody] Vcenter updated)
    {
        if (updated == null)
            return BadRequest("Vcenter payload is required.");

        if (id != updated.VcenterId)
            return BadRequest("Route id and payload VcenterId do not match.");

        var existing = await _db.Vcenters.FirstOrDefaultAsync(v => v.VcenterId == id);
        if (existing == null)
            return NotFound();

        // copy scalar values
        existing.Name = updated.Name;
        existing.IpAddress = updated.IpAddress;
        existing.Description = updated.Description;
        existing.IsActive = updated.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/vcenters/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVcenter(int id)
    {
        var vcenter = await _db.Vcenters.FirstOrDefaultAsync(v => v.VcenterId == id);
        if (vcenter == null)
            return NotFound();

        _db.Vcenters.Remove(vcenter);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}


/*  Original VCentersController.cs
 
 using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VcentersController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public VcentersController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/vcenters
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Vcenter>>> GetVcenters()
    {
        var vcenters = await _db.Vcenters
            .AsNoTracking()
            .OrderBy(v => v.IpAddress)   // or .OrderBy(v => v.Name) if you prefer
            .ToListAsync();

        return Ok(vcenters);
    }

    // (Optional later)
    // GET: api/vcenters/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Vcenter>> GetVcenter(int id)
    {
        var vcenter = await _db.Vcenters
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VcenterId == id);

        if (vcenter == null)
            return NotFound();

        return Ok(vcenter);
    }
}*/
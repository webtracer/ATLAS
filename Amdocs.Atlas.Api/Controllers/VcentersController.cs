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
}
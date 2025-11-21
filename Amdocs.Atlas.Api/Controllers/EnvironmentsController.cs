using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Environment = Amdocs.Atlas.Core.Entities.Environment;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnvironmentsController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public EnvironmentsController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/environments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Environment>>> GetEnvironments()
    {
        var envs = await _db.Environments
            .AsNoTracking()
            .OrderBy(e => e.Name)   // adjust if your property is different
            .ToListAsync();

        return Ok(envs);
    }
}
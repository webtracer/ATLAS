using Amdocs.Atlas.Core.DTOs;
using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public RolesController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.RoleId,   // or r.Id, depending on your Role entity
                Name = r.Name
            })
            .ToListAsync();

        return Ok(roles);
    }
}

/*using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public RolesController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/roles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        var roles = await _db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)   // adjust if needed
            .ToListAsync();

        return Ok(roles);
    }
}*/
using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Amdocs.Atlas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AtlasDbContext _db;

    public CustomersController(AtlasDbContext db)
    {
        _db = db;
    }

    // GET: api/customers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)   // adjust if needed
            .ToListAsync();

        return Ok(customers);
    }
}
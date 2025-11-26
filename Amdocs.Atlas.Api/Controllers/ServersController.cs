using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Amdocs.Atlas.Api.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;


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
    
    // POST: api/servers/import
[HttpPost("import")]
public async Task<ActionResult<ServerImportResult>> ImportServers(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("No file was uploaded.");

    var result = new ServerImportResult();

    try
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.First();

        // Assume headers in row 1
        var firstRow = 2;
        var lastRow = worksheet.LastRowUsed().RowNumber();

        // Preload lookup maps for Customers/Environments/Roles/Vcenters
        var customers = await _db.Customers.AsNoTracking().ToListAsync();
        var environments = await _db.Environments.AsNoTracking().ToListAsync();
        var roles = await _db.Roles.AsNoTracking().ToListAsync();
        var vcenters = await _db.Vcenters.AsNoTracking().ToListAsync();

        var customerByName = customers
            .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var envByName = environments
            .GroupBy(e => e.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var roleByName = roles
            .GroupBy(r => r.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // We'll match Vcenter by IP or Name, whatever matches first
        var vcenterByIp = vcenters
            .Where(v => !string.IsNullOrWhiteSpace(v.IpAddress))
            .GroupBy(v => v.IpAddress.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var vcenterByName = vcenters
            .Where(v => !string.IsNullOrWhiteSpace(v.Name))
            .GroupBy(v => v.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var serversToAdd = new List<Server>();

        result.TotalRows = lastRow - firstRow + 1;

        for (int rowNumber = firstRow; rowNumber <= lastRow; rowNumber++)
        {
            var row = worksheet.Row(rowNumber);

            string hostname = row.Cell("A").GetString().Trim();
            string vcenterKey = row.Cell("B").GetString().Trim(); // IP or Name
            string ipAddress = row.Cell("C").GetString().Trim();
            string customerName = row.Cell("D").GetString().Trim();
            string environmentName = row.Cell("E").GetString().Trim();
            string osName = row.Cell("F").GetString().Trim();
            string osFamily = row.Cell("G").GetString().Trim();
            string roleName = row.Cell("H").GetString().Trim();
            string fqdn = row.Cell("I").GetString().Trim();
            string locationName = row.Cell("J").GetString().Trim();
            string notes = row.Cell("K").GetString().Trim();

            // Skip completely empty rows
            if (string.IsNullOrWhiteSpace(hostname) &&
                string.IsNullOrWhiteSpace(ipAddress) &&
                string.IsNullOrWhiteSpace(customerName) &&
                string.IsNullOrWhiteSpace(environmentName))
            {
                continue;
            }

            // Basic validation
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(hostname))
                errors.Add("Hostname is required.");
            if (string.IsNullOrWhiteSpace(ipAddress))
                errors.Add("IP Address is required.");
            if (string.IsNullOrWhiteSpace(customerName))
                errors.Add("Customer is required.");
            if (string.IsNullOrWhiteSpace(environmentName))
                errors.Add("Environment is required.");
            if (string.IsNullOrWhiteSpace(roleName))
                errors.Add("Role is required.");

            if (errors.Count > 0)
            {
                result.Errors.Add(new ServerImportRowError
                {
                    RowNumber = rowNumber,
                    Message = string.Join(" ", errors)
                });
                continue;
            }

            if (!customerByName.TryGetValue(customerName, out var customer))
            {
                result.Errors.Add(new ServerImportRowError
                {
                    RowNumber = rowNumber,
                    Message = $"Unknown customer '{customerName}'."
                });
                continue;
            }

            if (!envByName.TryGetValue(environmentName, out var environment))
            {
                result.Errors.Add(new ServerImportRowError
                {
                    RowNumber = rowNumber,
                    Message = $"Unknown environment '{environmentName}'."
                });
                continue;
            }

            if (!roleByName.TryGetValue(roleName, out var role))
            {
                result.Errors.Add(new ServerImportRowError
                {
                    RowNumber = rowNumber,
                    Message = $"Unknown role '{roleName}'."
                });
                continue;
            }

            Vcenter? vcenter = null;
            if (!string.IsNullOrWhiteSpace(vcenterKey))
            {
                if (!vcenterByIp.TryGetValue(vcenterKey, out vcenter) &&
                    !vcenterByName.TryGetValue(vcenterKey, out vcenter))
                {
                    result.Errors.Add(new ServerImportRowError
                    {
                        RowNumber = rowNumber,
                        Message = $"Unknown vcenter '{vcenterKey}'."
                    });
                    continue;
                }
            }

            // Optional: lookup Location by name if you want to set LocationId
            int? locationId = null;
            if (!string.IsNullOrWhiteSpace(locationName))
            {
                var location = await _db.Locations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.Name == locationName);

                if (location != null)
                    locationId = location.LocationId;
            }

            var server = new Server
            {
                Hostname = hostname,
                VcenterId = vcenter?.VcenterId,
                IpAddress = ipAddress,
                CustomerId = customer.CustomerId,
                EnvironmentId = environment.EnvironmentId,
                OsName = osName,
                OsFamily = osFamily,
                Fqdn = fqdn,
                //Notes = notes,
                RoleId = role.RoleId,
                LocationId = locationId
            };

            serversToAdd.Add(server);
        }

        if (serversToAdd.Count > 0)
        {
            await _db.Servers.AddRangeAsync(serversToAdd);
            await _db.SaveChangesAsync();
        }

        result.ImportedCount = serversToAdd.Count;
        result.FailedCount = result.Errors.Count;

        return Ok(result);
    }
    catch (Exception ex)
    {
        // Generic failure
        return StatusCode(StatusCodes.Status500InternalServerError,
            new { message = "Failed to import servers.", detail = ex.Message });
    }
}

}

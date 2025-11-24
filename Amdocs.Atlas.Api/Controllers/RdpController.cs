using System.Text;
using System.Threading.Tasks;
using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Amdocs.Atlas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RdpController : ControllerBase
    {
        private readonly IServerRepository _serverRepository;

        public RdpController(IServerRepository serverRepository)
        {
            _serverRepository = serverRepository;
        }

        /// <summary>
        /// Returns a .rdp file for the given server id.
        /// Example route: GET /api/rdp/server/123
        /// </summary>
        [HttpGet("server/{id:int}")]
        public async Task<IActionResult> GetServerRdp(int id)
        {
            var server = await _serverRepository.GetByIdAsync(id);

            if (server is null || string.IsNullOrWhiteSpace(server.IpAddress))
            {
                return NotFound();
            }

            var rdpText = BuildRdpFile(server);

            // .rdp files are typically UTF-16 LE (what Encoding.Unicode gives you)
            var bytes = Encoding.Unicode.GetBytes(rdpText);

            var baseName = string.IsNullOrWhiteSpace(server.Hostname)
                ? server.IpAddress
                : server.Hostname;

            var fileName = $"{baseName}.rdp";

            // "application/x-rdp" is a common MIME type; octet-stream also works
            return File(bytes, "application/x-rdp", fileName);
        }

        private static string BuildRdpFile(Server server)
        {
            // Minimal RDP file content with "administrative session"
            var sb = new StringBuilder();

            sb.AppendLine("screen mode id:i:2");          // 2 = remote app / full screen
            sb.AppendLine("use multimon:i:0");
            sb.AppendLine("session bpp:i:32");
            sb.AppendLine($"full address:s:{server.IpAddress}");
            sb.AppendLine("prompt for credentials:i:1");
            sb.AppendLine("administrative session:i:1");  // equivalent to /admin
            sb.AppendLine("redirectclipboard:i:1");
            sb.AppendLine("redirectprinters:i:0");
            sb.AppendLine("redirectcomports:i:0");
            sb.AppendLine("redirectsmartcards:i:0");
            sb.AppendLine("redirectdrives:i:0");
            sb.AppendLine("redirectposdevices:i:0");

            return sb.ToString();
        }
    }
}

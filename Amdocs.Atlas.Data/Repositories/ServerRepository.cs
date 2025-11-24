using Amdocs.Atlas.Core.Entities;
using Amdocs.Atlas.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amdocs.Atlas.Data;

namespace Amdocs.Atlas.Data.Repositories
{
    public class ServerRepository : IServerRepository
    {
        private readonly AtlasDbContext _context;

        public ServerRepository(AtlasDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Server>> GetAllAsync()
        {
            return await _context.Servers.AsNoTracking().ToListAsync();
        }


        public Task<Server?> GetByIdAsync(int id)
            => _context.Servers.FindAsync(id).AsTask();

        public async Task AddAsync(Server server)
        {
            await _context.Servers.AddAsync(server);
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
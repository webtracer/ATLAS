using Amdocs.Atlas.Core.Entities;
using System.Threading.Tasks;

namespace Amdocs.Atlas.Core.Interfaces
{
    public interface IServerRepository
    {
        Task<IReadOnlyList<Server>> GetAllAsync();
        Task<Server?> GetByIdAsync(int id);

        Task AddAsync(Server server);
        Task SaveChangesAsync();
    }
}
using AutoMapper;
using Amdocs.Atlas.Core.DTOs;
using Amdocs.Atlas.Core.Entities;

namespace Amdocs.Atlas.Api.Mapping
{
    public class AtlasProfile : Profile
    {
        public AtlasProfile()
        {
            // Entity -> DTO
            CreateMap<Server, ServerDto>();

            // DTO -> Entity (for create)
            CreateMap<ServerCreateDto, Server>();
        }
    }
}
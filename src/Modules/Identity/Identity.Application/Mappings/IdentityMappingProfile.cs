using AutoMapper;
using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Mappings
{
    public class IdentityMappingProfile : Profile
    {
        public IdentityMappingProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        }
    }

}

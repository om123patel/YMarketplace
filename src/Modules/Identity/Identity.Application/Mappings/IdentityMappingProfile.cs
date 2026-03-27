using AutoMapper;
using Identity.Application.DTOs.Seller;
using Identity.Application.DTOs.User;
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


            CreateMap<Seller, SellerDto>()
              .ForMember(d => d.SellerStatus, o => o.MapFrom(s => s.Status.ToString()));
        }
    }

}

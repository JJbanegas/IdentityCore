using Application.ViewModels.Partners.v1;
using Application.ViewModels.Users;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Application.Mappings;

public class IdentityMapping : Profile
{
    public IdentityMapping()
    {
        CreateMap<object, PartnerGetAllResponseViewModel>().ReverseMap();

        CreateMap<IdentityUser, UserGetAllResponseViewModel>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
    }
}
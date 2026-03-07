using Application.ViewModels.Partners.v1;
using AutoMapper;

namespace Application.Mappings;

public class IdentityMapping : Profile
{
    public IdentityMapping()
    {
        CreateMap<object, PartnerGetAllResponseViewModel>().ReverseMap();
    }
}
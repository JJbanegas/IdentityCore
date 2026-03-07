using Application.Queries.Partners.v1;
using Application.ViewModels.Partners.v1;
using AutoMapper;
using Infrastructure.Interfaces;
using MediatR;

namespace Application.QueryHandlers.Partners.v1;

public class PartnerGetAllQueryHandler : IRequestHandler<PartnerGetAllQuery, List<PartnerGetAllResponseViewModel>>
{
    
    //private readonly IRepository<Partner> _partnerRepository;
    private readonly IMapper _mapper;

    public PartnerGetAllQueryHandler(/*IRepository<Partner> partnerRepository, */IMapper mapper)
    {
        //_partnerRepository = partnerRepository;
        _mapper = mapper;
    }

    public async Task<List<PartnerGetAllResponseViewModel>> Handle(PartnerGetAllQuery request, CancellationToken cancellationToken)
    {
        //var partnersList = (await _partnerRepository.GetAllAsync()).ToList();
        return _mapper.Map<List<PartnerGetAllResponseViewModel>>(new List<object>()/*partnersList*/);
    }
}
using Application.ViewModels.Partners.v1;
using MediatR;

namespace Application.Queries.Partners.v1;

public class PartnerGetAllQuery : IRequest<List<PartnerGetAllResponseViewModel>>
{
}
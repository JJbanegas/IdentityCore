using Application.ViewModels.Partners.v1;
using MediatR;

namespace Application.Commands.Partners.v1;

public class PartnerCreateCommand : IRequest<bool>
{
    public PartnerCreateCommand(PartnerCreateRequestViewModel model)
    {
        this.model = model;
    }
    public PartnerCreateRequestViewModel model { get; set; }
}
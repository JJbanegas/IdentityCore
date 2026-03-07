using Application.Commands.Partners.v1;
using Infrastructure.Interfaces;
using MediatR;

namespace Application.CommandHandlers.Partners.v1;

public class PartnerCreateCommandHandler : IRequestHandler<PartnerCreateCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    //private readonly IRepository<Partner> _partnerRepository;

    public PartnerCreateCommandHandler(//IRepository<Partner> partnerRepository,
        IUnitOfWork unitOfWork)
    {
        //_partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PartnerCreateCommand request, CancellationToken cancellationToken)
    {
        /*var newPartner = new Partner()
        {
            Name = request.model.Name,
            SurName = request.model.SurName,
            DateOfBirth = request.model.DateOfBirth,
            UserName = request.model.UserName,
            Password = request.model.Password
        };

        await _partnerRepository.AddAsync(newPartner);
        await _unitOfWork.SaveChangesAsync();*/
        return true;
    }
}
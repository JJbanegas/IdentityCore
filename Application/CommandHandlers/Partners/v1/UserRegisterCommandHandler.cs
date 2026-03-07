using Application.Commands.Partners.v1;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandHandlers.Partners.v1;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public UserRegisterCommandHandler( //IRepository<Partner> partnerRepository,
        IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
    {
        //_partnerRepository = partnerRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<bool> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _userManager.FindByEmailAsync(request.model.Email);
        if (userExists != null)
            //return new AuthResponse { Success = false, Message = "El usuario ya existe" };
            return false;

        var user = new IdentityUser
        {
            Email = request.model.Email,
            UserName = request.model.UserName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.model.Password);
        if (!result.Succeeded)
            //return new AuthResponse { Success = false, Message = string.Join(", ", result.Errors.Select(e => e.Description)) };
            return false;
        // Asignar rol por defecto
        await _userManager.AddToRoleAsync(user, "User");

        //return new AuthResponse { Success = true, Message = "Usuario registrado exitosamente" };
        return true;
    }
}
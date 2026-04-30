using Application.Commands.Partners.v1;
using Company.BuildingBlocks.Contracts.ErrorHandling.Exceptions;
using Company.BuildingBlocks.Contracts.Models;
using Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandHandlers.Partners.v1;

public class UserRegisterCommandHandler : IRequestHandler<UserRegisterCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<IdentityUser> _userManager;

    public UserRegisterCommandHandler(
        IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<string> Handle(UserRegisterCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _userManager.FindByEmailAsync(request.model.Email);
        if (userExists != null)
            throw new ConflictException("FindByEmailAsync", $"Ya existe un usuario registrado con el mail {request.model.Email}");

        var user = new IdentityUser
        {
            Email = request.model.Email,
            UserName = request.model.UserName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.model.Password);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
            throw new ConflictException("CreateAsync", errorMessage);
        }
        
        // Asignar rol por defecto
        await _userManager.AddToRoleAsync(user, "User");
        await _unitOfWork.SaveChangesAsync();

        return user.Id;
    }
}
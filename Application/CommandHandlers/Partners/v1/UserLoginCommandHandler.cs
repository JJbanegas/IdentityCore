using Application.Commands.Partners.v1;
using Application.ViewModels.Users;
using Company.BuildingBlocks.Contracts.ErrorHandling.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.CommandHandlers.Partners.v1;

public class UserLoginCommandHandler : IRequestHandler<UserLoginCommand, UserLoginResultViewModel>
{
    private readonly UserManager<IdentityUser> _userManager;

    public UserLoginCommandHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserLoginResultViewModel> Handle(UserLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.model.Email);
        if (user == null)
            throw new UnauthorizedException("Credenciales inválidas");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.model.Password);
        if (!passwordValid)
            throw new UnauthorizedException("Credenciales inválidas");

        var roles = await _userManager.GetRolesAsync(user);

        return new UserLoginResultViewModel
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}



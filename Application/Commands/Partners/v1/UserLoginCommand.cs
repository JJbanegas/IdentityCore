using Application.ViewModels.Users;
using MediatR;

namespace Application.Commands.Partners.v1;

public class UserLoginCommand : IRequest<UserLoginResultViewModel>
{
    public UserLoginCommand(UserLoginRequestViewModel model)
    {
        this.model = model;
    }

    public UserLoginRequestViewModel model { get; set; }
}


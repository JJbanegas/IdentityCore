using Application.ViewModels.Users;
using MediatR;

namespace Application.Commands.Partners.v1;

public class UserRegisterCommand : IRequest<string>
{
    public UserRegisterCommand(UserRegisterRequestViewModel model)
    {
        this.model = model;
    }
    public UserRegisterRequestViewModel model { get; set; }
}
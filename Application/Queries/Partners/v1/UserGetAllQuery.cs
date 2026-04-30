using Application.ViewModels.Users;
using MediatR;

namespace Application.Queries.Partners.v1;

public class UserGetAllQuery : IRequest<List<UserGetAllResponseViewModel>>
{
}


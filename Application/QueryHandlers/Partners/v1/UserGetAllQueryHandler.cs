using Application.Queries.Partners.v1;
using Application.ViewModels.Users;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.QueryHandlers.Partners.v1;

public class UserGetAllQueryHandler : IRequestHandler<UserGetAllQuery, List<UserGetAllResponseViewModel>>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMapper _mapper;

    public UserGetAllQueryHandler(UserManager<IdentityUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<List<UserGetAllResponseViewModel>> Handle(UserGetAllQuery request, CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        return _mapper.Map<List<UserGetAllResponseViewModel>>(users);
    }
}


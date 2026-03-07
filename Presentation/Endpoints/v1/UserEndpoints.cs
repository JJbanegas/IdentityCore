using Application.Commands.Partners.v1;
using Application.ViewModels.Users;
using IdentityCore.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCore.Endpoints.v1;

public class UserEndpoints : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/user")
            .WithOpenApi()
            .WithTags("User")
            .WithMetadata(new Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute {GroupName = "v1"})
            .WithGroupName("v1");

        /*group.MapGet("/GetAllPartners",
            async ([FromServices] IMediator mediator) => await mediator.Send(new PartnerGetAllQuery()));*/

        group.MapPost("/Register",
            async ([FromServices] IMediator mediator, [FromBody] UserRegisterRequestViewModel model) =>
            await mediator.Send(new UserRegisterCommand(model)));
    }
}
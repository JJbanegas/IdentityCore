using IdentityCore.Helpers;

namespace Presentation.Endpoints.v2;

public class PartnerEndpoints : IEndpointDefinition
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        Console.WriteLine("Registrando PartnerEndpoints v2...");
        
        var group = app.MapGroup("/api/v2/partner")
            .WithOpenApi()
            .WithTags("Partner")
            .WithMetadata(new Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute {GroupName = "v2"})
            .WithGroupName("v2");

        group.MapGet("/GetPartnerByUuid", () => { Results.Ok("Listado de usuarios"); });

    }
}
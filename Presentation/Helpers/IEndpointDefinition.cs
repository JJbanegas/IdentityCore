namespace IdentityCore.Helpers;

public interface IEndpointDefinition
{
    void MapEndpoints(IEndpointRouteBuilder app);
}

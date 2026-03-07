using System.Reflection;

namespace IdentityCore.Helpers;

public static class EndpointDefinitionExtensions
{
    public static void RegisterEndpointDefinitions(this WebApplication app)
    {
        var endpointDefinitions = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsInterface)
            .Select(Activator.CreateInstance)
            .Cast<IEndpointDefinition>();

        foreach (var endpointDefinition in endpointDefinitions)
        {
            endpointDefinition.MapEndpoints(app);
        }
    }
}

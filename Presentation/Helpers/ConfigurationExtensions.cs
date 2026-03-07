using Application.Commands.Partners.v1;
using Application.Mappings;
using Infrastructure;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdentityCore.Helpers;

public static class ConfigurationExtensions
{
    public static void SetConfigurationExtensions(IServiceCollection service)
    {
        service.AddDbContext<IdentityCoreDbContext>(options =>
                options.UseSqlServer(Environment.GetEnvironmentVariable("connectionstring")))
            .AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new() {Title = "Partners Core API", Version = "v1"});
                options.SwaggerDoc("v2", new() {Title = "Partners Core API", Version = "v2"});
            })
            .AddEndpointsApiExplorer()
            .AddAutoMapper(typeof(IdentityMapping))
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped(typeof(IRepository<>), typeof(Repository<>))
            .AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<PartnerCreateCommand>(); })
            .BuildServiceProvider();
    }
}
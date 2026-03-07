using IdentityCore.Helpers;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConfigurationExtensions = IdentityCore.Helpers.ConfigurationExtensions;

var builder = WebApplication.CreateBuilder(args);
ConfigurationExtensions.SetConfigurationExtensions(builder.Services);

builder.Services.AddDbContext<IdentityCoreDbContext>(options =>
    options.UseSqlServer(Environment.GetEnvironmentVariable("connectionstring")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityCoreDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Partners Core API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "Partners Core API v2");
    });
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityCoreDbContext>();
        dbContext.Database.Migrate();
    }
}

app.RegisterEndpointDefinitions();
app.UseHttpsRedirection();
//app.UseAuthentication();
//app.UseAuthorization();
app.Run();

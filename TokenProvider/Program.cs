using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TokenProvider.Infrastructure.Data.Contexts;
using TokenProvider.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddDbContextFactory<DataContext>(options =>
        {
            options.UseSqlServer(Environment.GetEnvironmentVariable("TOKEN_DATABASE"));
        });

        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
    })
    .Build();

host.Run();

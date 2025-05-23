namespace Katmai.WebApi;

using Katmai.Application;
using Katmai.Infrastructure;

public static class Program
{
    private const string SERVICE_NAME = "dms"; // container name from C4, C2 level
    private const string SERVICE_NAMESPACE = "consolia"; // system name from C4, C1 level

    private const int EXIT_SUCCESS = 0;
    public static async Task<int> Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.UseBootstrapper(SERVICE_NAME, SERVICE_NAMESPACE);

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseBootstrapper();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();

        return EXIT_SUCCESS;
    }
}

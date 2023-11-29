using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DeviceSimulator;
using DeviceSimulator.Infrastructure;
using Spectre.Console;
using Spectre.Console.Cli;

public static class Program
{
    public static int Main(string[] args)
    {
        AnsiConsole.Write(
            new FigletText("NothingWorx")
                .LeftJustified()
                .Color(Color.Blue));

        AnsiConsole.Write(
            new FigletText("DeviceSimulator")
                .Centered()
                .Color(Color.Blue));

        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", true);

        var rootConfig = configBuilder.Build();

        var registrations = new ServiceCollection();

        registrations.AddSingleton<IConfiguration>(rootConfig);

        // Create a type registrar and register any dependencies.
        // A type registrar is an adapter for a DI framework.
        var registrar = new TypeRegistrar(registrations);

        // Create a new command app with the registrar
        // and run it with the provided arguments.
        var app = new CommandApp<SimulateDevicesCommand>(registrar);

        app.Configure(config =>
        {
            config.AddCommand<SimulateDevicesCommand>("sim");
            config.AddCommand<IoTHubClearCommand>("clear");
        });

        return app.Run(args);
    }
}


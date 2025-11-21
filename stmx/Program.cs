using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

using stmx.Commands;
using stmx.Services;

namespace stmx;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddTransient<Command, DummyCommand>();
        services.AddTransient<Command, BatteryCommand>();
        services.AddTransient<DummyService>();
        services.AddTransient<SystemStatsService>();

        using var serviceProvider = services.BuildServiceProvider();

        RootCommand rootCommand = new("stmx: utility to get system stats for tmux.");
        serviceProvider
            .GetServices<Command>()
            .ToList()
            .ForEach(rootCommand.Add);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}

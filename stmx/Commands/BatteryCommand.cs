using System.CommandLine;

using stmx.Services;

namespace stmx.Commands;

class BatteryCommand : Command
{
    private readonly SystemStatsService _systemStats;

    public BatteryCommand(SystemStatsService systemStats) : base("battery", "get current battery status")
    {
        _systemStats = systemStats ?? throw new ArgumentNullException(nameof(systemStats));

        var showIconOption = new Option<bool>("--icon", ["-i"]);
        showIconOption.Description = "whether to show icon";
        showIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryIcon;
        Add(showIconOption);

        var showPercentOption = new Option<bool>("--percent", ["-p"]);
        showPercentOption.Description = "whether to percent symbol";
        showPercentOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryPercent;
        Add(showPercentOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var showIcon = parseResult.GetValue(showIconOption);
            var showPercent = parseResult.GetValue(showPercentOption);
            await ExecuteAsync(showIcon!, showPercent!);
        });
    }

    private async Task ExecuteAsync(bool showIcon, bool showPercent)
    {
        var batteryStatus = await _systemStats.GetBatteryStatus();
        // TODO: dynamic battery icon based on current value of battery
        Console.Write(
            $"{(showIcon ? "󰁹" : "")} {batteryStatus}{(showPercent ? "%" : "")}"
        );
    }
}

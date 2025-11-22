using System.CommandLine;

using stmx.Services;

namespace stmx.Commands;

class BatteryCommand : Command
{
    private readonly ISystemStatsService _systemStats;
    private readonly IconService _icons;

    public BatteryCommand(ISystemStatsService systemStats, IconService icons) : base("battery", "get current battery status")
    {
        _systemStats = systemStats ?? throw new ArgumentNullException(nameof(systemStats));
        _icons = icons ?? throw new ArgumentNullException(nameof(icons));

        var showIconOption = new Option<bool>("--icon", ["-i"]);
        showIconOption.Description = "whether to show icon";
        showIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryIcon;
        Add(showIconOption);

        var showChargingIconOption = new Option<bool>("--charging-icon", ["-c"]);
        showChargingIconOption.Description = "whether to show charging icon";
        showChargingIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryChargingIcon;
        Add(showChargingIconOption);

        var showPercentOption = new Option<bool>("--percent", ["-p"]);
        showPercentOption.Description = "whether to percent symbol";
        showPercentOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryPercent;
        Add(showPercentOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var showIcon = parseResult.GetValue(showIconOption);
            var showChargingIcon = parseResult.GetValue(showChargingIconOption);
            var showPercent = parseResult.GetValue(showPercentOption);
            await ExecuteAsync(showIcon!, showChargingIcon!, showPercent!);
        });
    }

    public async Task ExecuteAsync(bool showIcon, bool showStatusIcon, bool showPercent)
    {
        string batteryCapacityIcon = "", batteryStatusIcon = "", percentIcon = "";

        var batteryCapacity = await _systemStats.GetBatteryCapacity();
        if (batteryCapacity.HasValue) {
            batteryCapacityIcon = showIcon ? await _icons.GetBatteryCapacityIcon(batteryCapacity.Value) : "";
            percentIcon = showPercent ? "%" : "";
        }

        var batteryStatus = await _systemStats.GetBatteryStatus();
        if (batteryStatus.HasValue)
            batteryStatusIcon = showStatusIcon ? await _icons.GetBatteryStatusIcon(batteryStatus.Value) : "";



        Console.Write(
            $"{batteryCapacityIcon}{batteryStatusIcon} {batteryCapacity}{percentIcon}"
        );
    }
}

using System.CommandLine;

using stmx.Services;

namespace stmx.Commands;

class BatteryCommand : Command
{
    private readonly ISystemStatsService _systemStats;
    private readonly IIconService _icons;

    public BatteryCommand(ISystemStatsService systemStats, IIconService icons) : base("battery", "get current battery status")
    {
        _systemStats = systemStats ?? throw new ArgumentNullException(nameof(systemStats));
        _icons = icons ?? throw new ArgumentNullException(nameof(icons));

        var showIconOption = new Option<bool>("--show-icon", ["-i"]);
        showIconOption.Description = "show battery capacity icon";
        showIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryIcon;
        Add(showIconOption);

        var showStatusIconOption = new Option<bool>("--show-status-icon", ["-s"]);
        showStatusIconOption.Description = "show battery status icon";
        showStatusIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryChargingIcon;
        Add(showStatusIconOption);

        var showPercentIconOption = new Option<string>("--show-percent", ["-p"]);
        showPercentIconOption.Description = "show percent icon (optionally provide a custom icon)";
        showPercentIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showPercentIconOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var showIcon = parseResult.GetValue(showIconOption);
            var showStatusIcon = parseResult.GetValue(showStatusIconOption);
            var percentIconValue = parseResult.GetValue(showPercentIconOption);
            await ExecuteAsync(
                showIcon!,
                showStatusIcon!,
                parseResult.GetResult(showPercentIconOption) is not null,
                percentIconValue!
            );
        });
    }

    public async Task ExecuteAsync(
            bool showIcon,
            bool showStatusIcon,
            bool showPercentIcon,
            string percentIconValue)
    {
        string batteryCapacityIcon = "", batteryStatusIcon = "", percentIcon = "";

        var batteryCapacity = await _systemStats.GetBatteryCapacity();
        if (batteryCapacity.HasValue)
        {
            batteryCapacityIcon = showIcon ? await _icons.GetBatteryCapacityIcon(batteryCapacity.Value) : "";
            if (showPercentIcon)
            {
                percentIcon = string.IsNullOrEmpty(percentIconValue)
                    ? _icons.Options.PercentIcon
                    : percentIconValue;
            }
        }

        var batteryStatus = await _systemStats.GetBatteryStatus();
        if (batteryStatus.HasValue)
            batteryStatusIcon = showStatusIcon ? await _icons.GetBatteryStatusIcon(batteryStatus.Value) : "";

        string spacing = showIcon || showStatusIcon ? " ": "";
        Console.Write(
            $"{batteryCapacityIcon}{batteryStatusIcon}{spacing}{batteryCapacity}{percentIcon}"
        );
    }
}

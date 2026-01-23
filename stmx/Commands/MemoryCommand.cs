using System.CommandLine;

using stmx.Services;
using stmx.Utils;

namespace stmx.Commands;

class MemoryCommand : Command
{
    private readonly ISystemStatsService _systemStats;
    private readonly IIconService _icons;

    public MemoryCommand(ISystemStatsService systemStats, IIconService icons) : base("memory", "get current memory usage")
    {
        _systemStats = systemStats ?? throw new ArgumentNullException(nameof(systemStats));
        _icons = icons ?? throw new ArgumentNullException(nameof(icons));

        var showIconOption = new Option<string>("--icon", ["-i"]);
        showIconOption.Description = "whether to show icon (optionally provide custom icon)";
        // this allows users to pass one or more values
        showIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showIconOption);

        var showPercentOption = new Option<bool>("--percent", ["-p"]);
        showPercentOption.Description = "whether to show as a percentage";
        showPercentOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowBatteryPercent;
        Add(showPercentOption);

        var unitOption = new Option<MemoryUnits>("--unit", "-u");
        unitOption.Description = "Select the memory unit for display";
        unitOption.DefaultValueFactory = _ => _systemStats.Options.DefaultMemoryUnit;
        Add(unitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var iconValue = parseResult.GetValue(showIconOption);
            var showPercent = parseResult.GetValue(showPercentOption);
            var unit = parseResult.GetValue(unitOption);
            await ExecuteAsync(
                parseResult.GetResult(showIconOption) is not null,
                iconValue!,
                showPercent!,
                unit!
            );
        });
    }

    public async Task ExecuteAsync(bool showIcon, string iconValue, bool showPercent, MemoryUnits unit)
    {
        string memoryIcon = "";
        // if -i was passed
        if (showIcon)
        {
            memoryIcon = string.IsNullOrEmpty(iconValue)
                ? $"{_icons.Options.MemoryIcon} "
                : $"{iconValue} "; // if a custom icon was also passed
        }

        if (showPercent)
        {
            var memoryPercent = await _systemStats.GetMemoryUsagePercent(unit);
            Console.Write($"{memoryIcon}{memoryPercent:F2}%");
        }
        else
        {
            Console.Write($"{memoryIcon}{await _systemStats.GetMemoryUsageNumber(unit)}");
        }
    }
}

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

        var showIconOption = new Option<string>("--show-icon", ["-i"]);
        showIconOption.Description = "show icon (optionally provide custom icon)";
        // this allows users to pass one or more values
        showIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showIconOption);

        var showPercentIconOption = new Option<string>("--show-percent", ["-p"]);
        showPercentIconOption.Description = "show percent icon (optionally provide custom icon)";
        showPercentIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showPercentIconOption);

        var unitOption = new Option<MemoryUnits>("--unit", "-u");
        unitOption.Description = "Select the memory unit for display";
        unitOption.DefaultValueFactory = _ => _systemStats.Options.DefaultMemoryUnit;
        Add(unitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var iconValue = parseResult.GetValue(showIconOption);
            var percentValue = parseResult.GetValue(showPercentIconOption);
            var unit = parseResult.GetValue(unitOption);
            await ExecuteAsync(
                parseResult.GetResult(showIconOption) is not null,
                iconValue!,
                parseResult.GetResult(showPercentIconOption) is not null,
                percentValue!,
                unit!
            );
        });
    }

    public async Task ExecuteAsync(bool showIcon, string iconValue, bool showPercentIcon,
            string percentIconValue, MemoryUnits unit)
    {
        string memoryIcon = "", percentIcon = "";
        // if -i was passed
        if (showIcon)
        {
            memoryIcon = string.IsNullOrEmpty(iconValue)
                ? $"{_icons.Options.MemoryIcon} "
                : $"{iconValue} "; // if a custom icon was also passed
        }

        if (unit == MemoryUnits.Percent)
        {
            var memoryPercent = await _systemStats.GetMemoryUsagePercent();
            if (showPercentIcon)
            {
                percentIcon = string.IsNullOrEmpty(percentIconValue)
                    ? _icons.Options.PercentIcon
                    : percentIconValue;
            }
            Console.Write($"{memoryIcon}{memoryPercent:F2}{percentIcon}");
        }
        else
        {
            Console.Write($"{memoryIcon}{await _systemStats.GetMemoryUsageNumber(unit)}");
        }
    }
}

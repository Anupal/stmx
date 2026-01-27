using System.CommandLine;

using stmx.Services;

namespace stmx.Commands;

class CpuCommand : Command
{
    private readonly ISystemStatsService _systemStats;
    private readonly IIconService _icons;

    public CpuCommand(ISystemStatsService systemStats, IIconService icons) : base("cpu", "get current cpu usage")
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

        SetAction(async (parseResult, cancellationToken) =>
        {
            var iconValue = parseResult.GetValue(showIconOption);
            var percentIconValue = parseResult.GetValue(showPercentIconOption);
            await ExecuteAsync(
                parseResult.GetResult(showIconOption) is not null,
                iconValue!,
                parseResult.GetResult(showPercentIconOption) is not null,
                percentIconValue!
            );
        });
    }

    public async Task ExecuteAsync(bool showIcon, string iconValue, bool showPercentIcon, string percentIconValue)
    {
        string cpuIcon = "", percentIcon = "";
        // if -i was passed
        if (showIcon)
        {
            cpuIcon = string.IsNullOrEmpty(iconValue)
                ? $"{_icons.Options.CpuIcon} "
                : $"{iconValue} "; // if a custom icon was also passed
        }

        if (showPercentIcon)
        {
            percentIcon = string.IsNullOrEmpty(percentIconValue)
                ? _icons.Options.PercentIcon
                : percentIconValue;
        }

        var cpuPercent = await _systemStats.GetCpuUsagePercent();
        Console.Write($"{cpuIcon}{cpuPercent:F2}{percentIcon}");
    }
}

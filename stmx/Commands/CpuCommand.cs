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

        var showIconOption = new Option<string>("--icon", ["-i"]);
        showIconOption.Description = "whether to show icon (optionally provide custom icon)";
        // this allows users to pass one or more values
        showIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showIconOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var iconValue = parseResult.GetValue(showIconOption);
            await ExecuteAsync(
                parseResult.GetResult(showIconOption) is not null,
                iconValue!
            );
        });
    }

    public async Task ExecuteAsync(bool showIcon, string iconValue)
    {
        string cpuIcon = "";
        // if -i was passed
        if (showIcon)
        {
        cpuIcon = string.IsNullOrEmpty(iconValue)
            ? $"{_icons.Options.CpuIcon} "
            : $"{iconValue} "; // if a custom icon was also passed
        }

        var cpuPercent = await _systemStats.GetCpuUsagePercent();
        Console.Write($"{cpuIcon}{cpuPercent:F2}%");
    }
}

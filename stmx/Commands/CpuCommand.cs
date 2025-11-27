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

        var showIconOption = new Option<bool>("--icon", ["-i"]);
        showIconOption.Description = "whether to show icon";
        showIconOption.DefaultValueFactory = _ => _systemStats.Options.DefaultShowMemoryIcon;
        Add(showIconOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var showIcon = parseResult.GetValue(showIconOption);
            await ExecuteAsync(showIcon!);
        });
    }

    public async Task ExecuteAsync(bool showIcon)
    {
        string cpuIcon = showIcon ? $"{_icons.Options.CpuIcon} " : "";

        var cpuPercent = await _systemStats.GetCpuUsagePercent();
        Console.Write($"{cpuIcon}{cpuPercent:F2}%");
    }
}

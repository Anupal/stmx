using System.CommandLine;

using stmx.Services;
using stmx.Utils;

namespace stmx.Commands;

class NetworkCommand : Command
{
    private readonly ISystemStatsService _systemStats;
    private readonly IIconService _icons;

    public NetworkCommand(ISystemStatsService systemStats, IIconService icons) : base("network", "get current network usage")
    {
        _systemStats = systemStats ?? throw new ArgumentNullException(nameof(systemStats));
        _icons = icons ?? throw new ArgumentNullException(nameof(icons));

        var showIconOption = new Option<string>("--show-icon", ["-i"]);
        showIconOption.Description = "show icon (optionally provide custom icon)";
        // this allows users to pass one or more values
        showIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showIconOption);

        var directionOption = new Option<NetworkDirection>("--direction", "-d");
        directionOption.Description = "Show upload or download";
        directionOption.DefaultValueFactory = _ => _systemStats.Options.DefaultNetworkDirection;
        Add(directionOption);

        var unitOption = new Option<NetworkUnits>("--unit", "-u");
        unitOption.Description = "Select the network unit for display";
        unitOption.DefaultValueFactory = _ => _systemStats.Options.DefaultNetworkUnit;
        Add(unitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var iconValue = parseResult.GetValue(showIconOption);
            var direction = parseResult.GetValue(directionOption);
            var unit = parseResult.GetValue(unitOption);
            await ExecuteAsync(
                parseResult.GetResult(showIconOption) is not null,
                iconValue!,
                direction!,
                unit!
            );
        });
    }

    public async Task ExecuteAsync(bool showIcon, string iconValue, NetworkDirection direction,
            NetworkUnits unit)
    {
        string directionIcon = getDirectionIcon(showIcon, iconValue, direction);

        if (direction == NetworkDirection.Download)
        {

        }
        else
        {

        }
    }

    private string getDirectionIcon(bool showIcon, string iconValue, NetworkDirection direction) {
        if (showIcon)
        {
            return string.IsNullOrEmpty(iconValue)
                    ? $"{_icons.GetDirectionIcon(direction == NetworkDirection.Download)} "
                    : $"{iconValue} ";
        }
        return "";
    }
}

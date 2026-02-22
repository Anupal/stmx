using System.CommandLine;
using System.Text;

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

        var showDownloadIconOption = new Option<string>("--show-download-icon", ["-di"]);
        showDownloadIconOption.Description = "show download icon (optionally provide custom icon)";
        showDownloadIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showDownloadIconOption);

        var showUploadIconOption = new Option<string>("--show-upload-icon", ["-ui"]);
        showUploadIconOption.Description = "show upload icon (optionally provide custom icon)";
        // this allows users to pass one or more values
        showUploadIconOption.Arity = ArgumentArity.ZeroOrOne;
        Add(showUploadIconOption);

        var directionOption = new Option<NetworkDirection>("--direction", "-d");
        directionOption.Description = "Show upload or download";
        directionOption.DefaultValueFactory = _ => _systemStats.Options.DefaultNetworkDirection;
        Add(directionOption);

        // option to specify network interface
        var networkOption = new Option<string>("--network", "-n");
        networkOption.Description = "Select the network interface";
        networkOption.Required = true;
        networkOption.DefaultValueFactory = _ => "default";
        Add(networkOption);

        // option to specify sample delay
        var delayOption = new Option<int>("--time-delay", "-t");
        delayOption.Description = "Set the delay for sampling network speeds in seconds";
        delayOption.DefaultValueFactory = _ => _systemStats.Options.DefaultNetworkDelay;
        delayOption.Validators.Add(result =>
        {
            var value = result.GetValueOrDefault<int>();
            if (value <= 0)
                result.AddError("Delay must be greater than 0");
        });
        Add(delayOption);

        var unitOption = new Option<NetworkUnits>("--unit", "-u");
        unitOption.Description = "Select the network unit for display";
        unitOption.DefaultValueFactory = _ => _systemStats.Options.DefaultNetworkUnit;
        Add(unitOption);

        SetAction(async (parseResult, cancellationToken) =>
        {
            var uploadIconValue = parseResult.GetValue(showUploadIconOption);
            var downloadIconValue = parseResult.GetValue(showDownloadIconOption);
            var direction = parseResult.GetValue(directionOption);
            var network = parseResult.GetValue(networkOption);
            var delay = parseResult.GetValue(delayOption);
            var unit = parseResult.GetValue(unitOption);
            await ExecuteAsync(
                parseResult.GetResult(showUploadIconOption) is not null,
                uploadIconValue!,
                parseResult.GetResult(showDownloadIconOption) is not null,
                downloadIconValue!,
                direction!,
                network!,
                delay!,
                unit!
            );
        });
    }

    public async Task ExecuteAsync(
            bool showUploadIcon,
            string uploadIconValue,
            bool showDownloadIcon,
            string downloadIconValue,
            NetworkDirection direction,
            string network,
            int delay,
            NetworkUnits unit)
    {


        var dataSpeed = await _systemStats.GetNetworkSpeed(unit, network, delay);
        var commandOutputSb = new StringBuilder();

        if (direction is NetworkDirection.Download or NetworkDirection.Both)
        {
            string downloadIcon = GetDirectionIcon(showDownloadIcon, downloadIconValue, NetworkDirection.Download);
            commandOutputSb.Append($"{downloadIcon}{dataSpeed.Download.Value:F2}{dataSpeed.Download.UnitToString()}");
        }
        if (direction is NetworkDirection.Both)
            commandOutputSb.Append(" ");
        if (direction is NetworkDirection.Upload or NetworkDirection.Both)
        {
            string uploadIcon = GetDirectionIcon(showUploadIcon, uploadIconValue, NetworkDirection.Upload);
            commandOutputSb.Append($"{uploadIcon}{dataSpeed.Upload.Value:F2}{dataSpeed.Upload.UnitToString()}");
        }

        System.Console.Write(commandOutputSb.ToString());
    }

    private string GetDirectionIcon(bool showIcon, string iconValue, NetworkDirection direction) {
        if (showIcon)
        {
            var defaultIcon = direction == NetworkDirection.Upload
                ? _icons.Options.NetworkUpload
                : _icons.Options.NetworkDownload;

            return string.IsNullOrEmpty(iconValue)
                    ? $"{defaultIcon} "
                    : $"{iconValue} ";
        }
        return "";
    }
}

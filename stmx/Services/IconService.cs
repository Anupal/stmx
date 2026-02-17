using stmx.Utils;

namespace stmx.Services;

class IconService : IIconService
{
    public IconServiceOptions Options { get; } = new IconServiceOptions();

    public Task<string> GetBatteryCapacityIcon(int batteryCapacity)
    {
        var iconIndex = (int)Math.Floor((double)(batteryCapacity / 10));
        return Task.FromResult(Options.BatteryCapacityIcons[iconIndex]);
    }

    public Task<string> GetBatteryStatusIcon(int batteryStatus) {
        return Task.FromResult(Options.BatteryStatusIcons[batteryStatus]);
    }

    public Task<string> GetDirectionIcon(bool download)
    {
        return Task.FromResult(
            (download) ? Options.NetworkDownload : Options.NetworkUpload
        );
    }
}

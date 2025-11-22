namespace stmx.Services;

class IconService()
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
}

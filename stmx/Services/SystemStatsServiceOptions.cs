using stmx.Utils;

namespace stmx.Services;

public class SystemStatsServiceOptions
{
    public bool DefaultShowBatteryIcon { get; set; } = false;
    public bool DefaultShowBatteryChargingIcon { get; set; } = false;
    public bool DefaultShowBatteryPercent { get; set; } = false;

    public MemoryUnits DefaultMemoryUnit { get; set; } = MemoryUnits.Percent;
    public NetworkUnits DefaultNetworkUnit { get; set; } = NetworkUnits.Auto;
    public int DefaultNetworkDelay { get; set; } = 3;
    public NetworkDirection DefaultNetworkDirection { get; set; } = NetworkDirection.Download;
}

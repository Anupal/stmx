using stmx.Utils;

namespace stmx.Services;

public class SystemStatsServiceOptions
{
    public bool DefaultShowBatteryIcon { get; set; } = false;
    public bool DefaultShowBatteryChargingIcon { get; set; } = false;
    public bool DefaultShowBatteryPercent { get; set; } = false;

    public MemoryUnits DefaultMemoryUnit {get; set; } = MemoryUnits.Percent;
}

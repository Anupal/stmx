namespace stmx.Services;

public class LinuxSystemStatsService : ISystemStatsService
{
    public SystemStatsServiceOptions Options { get; } = new SystemStatsServiceOptions();

    public Task<int?> GetBatteryCapacity()
    {
        return Task.FromResult(ReadBatteryCapacityFromSysFile());
    }

    protected virtual int? ReadBatteryCapacityFromSysFile()
    {
        try
        {
            string batteryCapacity = File.ReadAllText("/sys/class/power_supply/BAT1/capacity");
            return int.Parse(batteryCapacity);
        }
        catch (IOException)
        {
            // TODO: print exception if debug is true
            return null;
        }
    }

    public Task<int?> GetBatteryStatus() {
        return Task.FromResult(ReadBatteryStatusFromSysFile());
    }

    public int? ReadBatteryStatusFromSysFile()
    {
        try
        {
            string batteryStatus = File.ReadAllText("/sys/class/power_supply/BAT1/status").Trim('\n', '\r');
            if (batteryStatus == "Charging" || batteryStatus == "Full")
                return 1;
            else if (batteryStatus == "Discharging")
                return 0;
            return 2;
        }
        catch (IOException)
        {
            // TODO: print exception if debug is true
            return null;
        }
    }
}

using stmx.Utils;

namespace stmx.Services;

public class LinuxSystemStatsService : ISystemStatsService
{
    private readonly IFileReader _fileReader;

    public LinuxSystemStatsService(IFileReader fileReader)
    {
        _fileReader = fileReader;
    }

    public SystemStatsServiceOptions Options { get; } = new SystemStatsServiceOptions();

    public async Task<int?> GetBatteryCapacity()
    {
        try
        {
            return ReadBatteryCapacityFromSysFile();
        }
        catch (IOException)
        {
            return null;
        }
    }

    public int? ReadBatteryCapacityFromSysFile()
    {
        string batteryCapacity = _fileReader.ReadAllText("/sys/class/power_supply/BAT1/capacity");
        return int.Parse(batteryCapacity);
    }

    public async Task<int?> GetBatteryStatus() {
        try
        {
            return ReadBatteryStatusFromSysFile();
        }
        catch (IOException)
        {
            return null;
        }
    }

    public int? ReadBatteryStatusFromSysFile()
    {
        string batteryStatus = _fileReader.ReadAllText("/sys/class/power_supply/BAT1/status").Trim('\n', '\r');
        if (batteryStatus == "Charging" || batteryStatus == "Full")
            return 1;
        else if (batteryStatus == "Discharging")
            return 0;
        return 2;
    }
}

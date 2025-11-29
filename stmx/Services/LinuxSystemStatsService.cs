using System.Text.RegularExpressions;

using stmx.Utils;

namespace stmx.Services;

public class LinuxSystemStatsService : ISystemStatsService
{
    private readonly IFileReader _fileReader;
    private readonly IFileSystem _fileSystem;

    public LinuxSystemStatsService(IFileReader fileReader, IFileSystem fileSystem)
    {
        _fileReader = fileReader;
        _fileSystem = fileSystem;
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
        string? batteryDir = GetBatterySysDirectory();
        if (batteryDir == null)
            throw new IOException("No directory found for battery in /sys/class/power_supply");

        string batteryCapacity = _fileReader.ReadAllText($"/sys/class/power_supply/{batteryDir}/capacity");
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
        string? batteryDir = GetBatterySysDirectory();
        if (batteryDir == null)
            throw new IOException("No directory found for battery in /sys/class/power_supply");

        string batteryStatus = _fileReader.ReadAllText($"/sys/class/power_supply/{batteryDir}/status").Trim('\n', '\r');
        if (batteryStatus == "Charging" || batteryStatus == "Full")
            return 1;
        else if (batteryStatus == "Discharging")
            return 0;
        return 2;
    }

    public string? GetBatterySysDirectory()
    {
        var basePath = "/sys/class/power_supply";
        if (!_fileSystem.DirectoryExists(basePath))
            return null;

        var dirs = _fileSystem.GetDirectories(basePath)
                        .Select(Path.GetFileName)
                        .Where(name => name != null && name.StartsWith("BAT", StringComparison.OrdinalIgnoreCase))
                        .ToList();

        return dirs.FirstOrDefault();
    }

    public async Task<string?> GetMemoryUsageNumber(MemoryUnits unit)
    {
        try
        {
            MemoryUsageData data = ReadMemoryUsageFromProcFile().ConvertTo(unit);
            return $"{data.Used} / {data.Total}";
        }
        catch (IOException)
        {
            return null;
        }
    }

    public async Task<double?> GetMemoryUsagePercent(MemoryUnits unit)
    {
        try
        {
            MemoryUsageData data = ReadMemoryUsageFromProcFile().ConvertTo(unit);
            double percent = ((double)data.Used / data.Total) * 100;
            return Math.Round(percent, 2);
        }
        catch (IOException)
        {
            return null;
        }
    }

    // This data is always in KiloBytes
    public MemoryUsageData ReadMemoryUsageFromProcFile()
    {
        string totalMemoryPattern = @"MemTotal:\s+(\d+)";
        string availableMemoryPattern = @"MemAvailable:\s+(\d+)";

        string memoryInfo = _fileReader.ReadAllText("/proc/meminfo");

        long totalMemory = long.Parse(Regex.Matches(memoryInfo, totalMemoryPattern)[0].Groups[1].Value);
        long availableMemory = long.Parse(Regex.Matches(memoryInfo, availableMemoryPattern)[0].Groups[1].Value);

        return new MemoryUsageData(totalMemory, availableMemory, MemoryUnits.KiloBytes);
    }

    public async Task<double?> GetCpuUsagePercent()
    {
        int delayMs = 100;

        try
        {
            var oldValue = ReadCpuUsageFromProcFile();
            await Task.Delay(delayMs);
            var newValue = ReadCpuUsageFromProcFile();

            // compute cpu usage
            long idleOld = oldValue.Idle + oldValue.IOWait;
            long idleNew = newValue.Idle + newValue.IOWait;

            long nonIdleOld = oldValue.User + oldValue.Nice + oldValue.System +
                              oldValue.IRQ + oldValue.SoftIRQ + oldValue.Steal;

            long nonIdleNew = newValue.User + newValue.Nice + newValue.System +
                              newValue.IRQ + newValue.SoftIRQ + newValue.Steal;

            long totalOld = idleOld + nonIdleOld;
            long totalNew = idleNew + nonIdleNew;

            long totalDiff = totalNew - totalOld;
            long idleDiff = idleNew - idleOld;

            // avoid div by 0
            if (totalDiff == 0)
                return 0.0;

            var percent = (double)(totalDiff - idleDiff) / totalDiff * 100.0;
            return Math.Round(percent, 2);
        }
        catch (IOException)
        {
            return null;
        }
    }

    public CpuTimesData ReadCpuUsageFromProcFile()
    {
        string cpuTimesPattern = @"cpu\s+(\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+)";

        string cpuInfo = _fileReader.ReadAllText("/proc/stat");

        var values = Regex.Matches(cpuInfo, cpuTimesPattern)[0].Groups;

        return new CpuTimesData
        {
            User = long.Parse(values[1].Value),
            Nice = long.Parse(values[2].Value),
            System = long.Parse(values[3].Value),
            Idle = long.Parse(values[4].Value),
            IOWait = long.Parse(values[5].Value),
            IRQ = long.Parse(values[6].Value),
            SoftIRQ = long.Parse(values[7].Value),
            Steal = long.Parse(values[8].Value),
            Guest = long.Parse(values[9].Value),
            GuestNice = long.Parse(values[10].Value),
        };
    }
}

public class CpuTimesData
{
    public long User { set; get; }
    public long Nice { set; get; }
    public long System { set; get; }
    public long Idle { set; get; }
    public long IOWait {set; get; }
    public long IRQ { set; get; }
    public long SoftIRQ { set; get; }
    public long Steal { get; set; }
    public long Guest { get; set; }
    public long GuestNice { get; set; }
}

public class MemoryUsageData
{
    public long Total { set; get; }
    public long Available { set; get; }
    public long Used { set; get; }
    public MemoryUnits Unit { set; get; }

    public MemoryUsageData(long total, long available, MemoryUnits unit)
    {
        Total = total;
        Available = available;
        Used = total - available;
        Unit = unit;
    }

    public MemoryUsageData ConvertTo(MemoryUnits targetUnit)
    {
        return new MemoryUsageData(
            ConvertValue(Total, Unit, targetUnit),
            ConvertValue(Available, Unit, targetUnit),
            targetUnit
        );
    }

    private static long ConvertValue(long value, MemoryUnits from, MemoryUnits to)
    {
        double bytes = value * UnitToBytesFactor(from);     // → convert to bytes
        double result = bytes / UnitToBytesFactor(to);      // → convert from bytes to target

        return (long)Math.Round(result, 2);
    }

    private static double UnitToBytesFactor(MemoryUnits unit)
    {
        return unit switch
        {
            MemoryUnits.Bytes     => 1,
            MemoryUnits.KiloBytes => 1_000,
            MemoryUnits.KibiBytes => 1_024,

            MemoryUnits.MegaBytes => 1_000_000,
            MemoryUnits.MibiBytes => 1_048_576,

            MemoryUnits.GigaBytes => 1_000_000_000,
            MemoryUnits.GibiBytes => 1_073_741_824,

            MemoryUnits.TeraBytes => 1_000_000_000_000,
            MemoryUnits.TibiBytes => 1_099_511_627_776,

            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };
    }
}

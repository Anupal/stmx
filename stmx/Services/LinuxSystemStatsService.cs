using System.Text.RegularExpressions;

using stmx.Utils;

namespace stmx.Services;

public class LinuxSystemStatsService : ISystemStatsService
{
    private readonly IFileReader _fileReader;
    private readonly IFileSystem _fileSystem;
    private readonly Regex TotalMemoryRegex = new (@"MemTotal:\s+(\d+)");
    private readonly Regex AvailableMemoryRegex = new (@"MemAvailable:\s+(\d+)");
    private readonly Regex CpuTimesRegex = new (@"cpu\s+(\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+) (\d+)");

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

    public async Task<double?> GetMemoryUsagePercent()
    {
        try
        {
            MemoryUsageData data = ReadMemoryUsageFromProcFile();
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
        string memoryInfo = _fileReader.ReadAllText("/proc/meminfo");

        long totalMemory    = long.Parse(TotalMemoryRegex.Match(memoryInfo).Groups[1].Value);
        long availableMemory = long.Parse(AvailableMemoryRegex.Match(memoryInfo).Groups[1].Value);

        return new MemoryUsageData(totalMemory, availableMemory, MemoryUnits.KiloBytes);
    }

    public async Task<(NetworkSpeed Upload, NetworkSpeed Download)> GetNetworkSpeed(
            NetworkUnits unit, string network, int delaySecs)
    {
        // take two samples
        var dataOld = ReadNetworkSpeedFromSysFile(network);
        await Task.Delay(delaySecs * 1000);
        var dataNew = ReadNetworkSpeedFromSysFile(network);

        // compute speed in bits per second
        // If the counter was reset to 0 for second sample
        // set the diff to 0 using Math.Max() to avoid negative value
        var uploadSpeed = new NetworkSpeed(
                Math.Max(0, dataNew.Upload - dataOld.Upload) / delaySecs,
                NetworkUnits.Bits
        );
        var downloadSpeed = new NetworkSpeed(
                Math.Max(0, dataNew.Download - dataOld.Download) / delaySecs,
                NetworkUnits.Bits
        );

        // auto or explicit conversion
        if (unit == NetworkUnits.Auto)
        {
            return (uploadSpeed.ConvertAuto(), downloadSpeed.ConvertAuto());
        }
        else
        {
            return (uploadSpeed.ConvertTo(unit), downloadSpeed.ConvertTo(unit));
        }
    }

    // Returns current values of tx and rx counters
    public (long Upload, long Download) ReadNetworkSpeedFromSysFile(string network_interface = "wlo1")
    {
        if (network_interface == "default")
            network_interface = GetDefaultNetworkInterface();

        string basePath = $"/sys/class/net/{network_interface}";
        if (!_fileSystem.DirectoryExists(basePath))
            throw new IOException($"No entry found for network {network_interface}");

        var upload = long.Parse(_fileReader.ReadAllText(
                    $"/sys/class/net/{network_interface}/statistics/tx_bytes")) * 8;
        var download = long.Parse(_fileReader.ReadAllText(
                    $"/sys/class/net/{network_interface}/statistics/rx_bytes")) * 8;

        return (upload, download);
    }

    public string GetDefaultNetworkInterface()
    {
        var lines = _fileReader.ReadAllText("/proc/net/route").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        // skip header line, find route with destination 00000000 (default)
        foreach (var line in lines.Skip(1))
        {
            var fields = line.Split('\t');
            if (fields.Length > 1 && fields[1] == "00000000")
                return fields[0];
        }
        throw new IOException("No default network interface found");
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
        string cpuInfo = _fileReader.ReadAllText("/proc/stat");

        // var values = Regex.Matches(cpuInfo, cpuTimesPattern)[0].Groups;
        var values = CpuTimesRegex.Match(cpuInfo).Groups;

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

public class NetworkSpeed
{
    public double Value { set; get; }
    public NetworkUnits Unit { set; get; }

    public NetworkSpeed(double value, NetworkUnits unit)
    {
        Value = value;
        Unit = unit;
    }

    public NetworkSpeed ConvertTo(NetworkUnits targetUnit)
    {
        return new NetworkSpeed(
            ConvertValue(Value, Unit, targetUnit),
            targetUnit
        );
    }

    // Assumes that upload and download are in bits
    public NetworkSpeed ConvertAuto()
    {
        NetworkUnits targetUnit = Value switch
        {
            >= 1_099_511_627_776 => NetworkUnits.TeraBits,
            >= 1_073_741_824     => NetworkUnits.GigaBits,
            >= 1_048_576         => NetworkUnits.MegaBits,
            _             => NetworkUnits.KiloBits
            // not including bits per second with Auto option
        };

        return ConvertTo(targetUnit);
    }

    private static double ConvertValue(double value, NetworkUnits from, NetworkUnits to)
    {
        double bits = value * UnitToBitsFactor(from);
        double result = bits / UnitToBitsFactor(to);

        return result;
    }

    private static double UnitToBitsFactor(NetworkUnits unit)
    {
        return unit switch
        {
            NetworkUnits.Bits     => 1,
            NetworkUnits.KiloBits => 1_024,
            NetworkUnits.MegaBits => 1_048_576,
            NetworkUnits.GigaBits => 1_073_741_824,
            NetworkUnits.TeraBits => 1_099_511_627_776,

            _ => throw new ArgumentOutOfRangeException(nameof(unit))
        };
    }

    public string UnitToString()
    {
        return Unit switch
        {
            NetworkUnits.Bits     => "b",
            NetworkUnits.KiloBits => "k",
            NetworkUnits.MegaBits => "m",
            NetworkUnits.GigaBits => "g",
            NetworkUnits.TeraBits => "t",

            _ => throw new ArgumentOutOfRangeException(nameof(Unit))
        };
    }
}

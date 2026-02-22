using stmx.Utils;

namespace stmx.Services;

public interface ISystemStatsService
{
    Task<int?> GetBatteryCapacity();
    Task<int?> GetBatteryStatus();
    Task<string?> GetMemoryUsageNumber(MemoryUnits unit);
    Task<double?> GetMemoryUsagePercent();
    Task<(NetworkSpeed Upload, NetworkSpeed Download)> GetNetworkSpeed(
            NetworkUnits unit, string network, int delaySecs);
    Task<double?> GetCpuUsagePercent();
    SystemStatsServiceOptions Options { get; }
}

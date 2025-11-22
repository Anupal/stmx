namespace stmx.Services;

public interface ISystemStatsService
{
    Task<int?> GetBatteryCapacity();
    Task<int?> GetBatteryStatus();
    SystemStatsServiceOptions Options { get; }
}

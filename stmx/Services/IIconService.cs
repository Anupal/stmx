namespace stmx.Services;

public interface IIconService
{
    Task<string> GetBatteryCapacityIcon(int batteryCapacity);
    Task<string> GetBatteryStatusIcon(int batteryStatus);
    IconServiceOptions Options { get; }
}

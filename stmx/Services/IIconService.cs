namespace stmx.Services;

public interface IIconService
{
    Task<string> GetBatteryCapacityIcon(int batteryCapacity);
    Task<string> GetBatteryStatusIcon(int batteryStatus);
    Task<string> GetDirectionIcon(bool download);
    IconServiceOptions Options { get; }
}

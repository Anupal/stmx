namespace stmx.Services;

class SystemStatsService()
{
    public SystemStatsServiceOptions Options { get; } = new SystemStatsServiceOptions();

    public Task<int> GetBatteryStatus()
    {
        return Task.FromResult(50);
    }

    private Task<bool> IsBatteryCharging()
    {
        return Task.FromResult(false);
    }
}

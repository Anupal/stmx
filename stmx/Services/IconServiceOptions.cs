namespace stmx.Services;

public class IconServiceOptions
{
    public List<string> BatteryStatusIcons { get; set; } = new()
    {
        "", "󱐋", "" // discharging, charging, full, unknown
    };
    public List<string> BatteryCapacityIcons { get; set; } = new()
    {
        "󰂎", "󰁺", "󰁻", "󰁼", "󰁽", "󰁾", "󰁿", "󰂀", "󰂁", "󰂂", "󰁹",
    };
    public string MemoryIcon { get; set; } = "";
    public string CpuIcon { get; set; } = "";
}

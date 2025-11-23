namespace stmx.Services;

class IconServiceOptions
{
    public List<string> BatteryStatusIcons { get; set; } = new()
    {
        "", "󱐋", "" // discharging, charging, full, unknown
    };
    public List<string> BatteryCapacityIcons { get; set; } = new()
    {
        "󰂎", "󰁺", "󰁻", "󰁼", "󰁽", "󰁾", "󰁿", "󰂀", "󰂁", "󰂂", "󰁹",
    };
}

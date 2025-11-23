using NUnit.Framework;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

using stmx.Commands;
using stmx.Services;

namespace stmx.Tests;

public class BatteryCommandTests
{
    [Test]
    public async Task TestBatteryCommand_PrintsIconsAndPercent()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetBatteryCapacity()).ReturnsAsync(80);
        mockIconService.Setup(s => s.GetBatteryCapacityIcon(80)).ReturnsAsync("CAP_ICON");
        mockStatsService.Setup(s => s.GetBatteryStatus()).ReturnsAsync(1);
        mockIconService.Setup(s => s.GetBatteryStatusIcon(1)).ReturnsAsync("STATUS_ICON");
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());

        var cmd = new BatteryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: true, showStatusIcon: true, showPercent: true);

        Assert.That(consoleOut.ToString(), Is.EqualTo("CAP_ICONSTATUS_ICON 80%"));
    }

    [Test]
    public async Task TestBatteryCommand_NoBatteryData_PrintsEmptyFields()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetBatteryCapacity()).ReturnsAsync((int?)null);
        mockStatsService.Setup(s => s.GetBatteryStatus()).ReturnsAsync((int?)null);
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());

        var cmd = new BatteryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: false, showStatusIcon: false, showPercent: false);

        Assert.That(consoleOut.ToString(), Is.EqualTo(" "));
    }

    [Test]
    public async Task TestBatteryCommand_RespectsDefaultOptions()
    {
        var opts = new SystemStatsServiceOptions
        {
            DefaultShowBatteryIcon = true,
            DefaultShowBatteryChargingIcon = false,
            DefaultShowBatteryPercent = true
        };

        var mockStatsService = new Mock<ISystemStatsService>();
        mockStatsService.SetupGet(s => s.Options).Returns(opts);
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetBatteryCapacity()).ReturnsAsync(50);
        mockIconService.Setup(s => s.GetBatteryCapacityIcon(50)).ReturnsAsync("ICON50");
        mockStatsService.Setup(s => s.GetBatteryStatus()).ReturnsAsync(0);
        mockIconService.Setup(s => s.GetBatteryStatusIcon(0)).ReturnsAsync("");

        var cmd = new BatteryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        // Use defaults
        await cmd.ExecuteAsync(showIcon: opts.DefaultShowBatteryIcon,
                               showStatusIcon: opts.DefaultShowBatteryChargingIcon,
                               showPercent: opts.DefaultShowBatteryPercent);

        Assert.That(consoleOut.ToString(), Is.EqualTo("ICON50 50%"));
    }
}


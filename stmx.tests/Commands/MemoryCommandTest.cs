using Moq;

using stmx.Commands;
using stmx.Services;
using stmx.Utils;

namespace stmx.Tests;

public class MemoryCommandTests
{
    [Test]
    public async Task TestMemoryCommand_PrintsIconAndPercent()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetMemoryUsagePercent(MemoryUnits.MibiBytes)).ReturnsAsync(75.0);
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        });
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON"
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: true, showPercent: true, unit: MemoryUnits.MibiBytes);

        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON 75%"));
    }

    [Test]
    public async Task TestMemoryCommand_PrintsNumberWithoutIcon()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetMemoryUsageNumber(MemoryUnits.GibiBytes))
                        .ReturnsAsync("2 / 8");
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.GibiBytes
        });
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON"
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: false, showPercent: false, unit: MemoryUnits.GibiBytes);

        Assert.That(consoleOut.ToString(), Is.EqualTo("2 / 8"));
    }

    [Test]
    public async Task TestMemoryCommand_RespectsDefaultOptions()
    {
        var opts = new SystemStatsServiceOptions
        {
            DefaultShowMemoryIcon = true,
            DefaultShowBatteryPercent = true,
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        };

        var mockStatsService = new Mock<ISystemStatsService>();
        mockStatsService.SetupGet(s => s.Options).Returns(opts);
        var mockIconService = new Mock<IIconService>();
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON"
        });

        mockStatsService.Setup(s => s.GetMemoryUsagePercent(MemoryUnits.MibiBytes)).ReturnsAsync(60.0);

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        // Using defaults
        await cmd.ExecuteAsync(
            showIcon: opts.DefaultShowMemoryIcon,
            showPercent: opts.DefaultShowBatteryPercent,
            unit: opts.DefaultMemoryUnit
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON 60%"));
    }

    [Test]
    public async Task TestMemoryCommand_HandlesNullValues()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetMemoryUsagePercent(MemoryUnits.MibiBytes)).ReturnsAsync((double?)null);
        mockStatsService.Setup(s => s.GetMemoryUsageNumber(MemoryUnits.MibiBytes)).ReturnsAsync((string?)null);

        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        });
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON"
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        // Percent null
        await cmd.ExecuteAsync(showIcon: true, showPercent: true, unit: MemoryUnits.MibiBytes);
        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON %"));

        consoleOut.GetStringBuilder().Clear();

        // Number null
        await cmd.ExecuteAsync(showIcon: true, showPercent: false, unit: MemoryUnits.MibiBytes);
        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON "));
    }
}


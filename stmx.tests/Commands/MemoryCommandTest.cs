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

        mockStatsService.Setup(s => s.GetMemoryUsagePercent()).ReturnsAsync(75.0);
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
                showIcon: true,
                iconValue: "MEM_ICON",
                showPercentIcon: true,
                percentIconValue: "%",
                unit: MemoryUnits.Percent);

        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON 75.00%"));
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

        await cmd.ExecuteAsync(
                showIcon: false,
                iconValue: "",
                showPercentIcon: true,
                percentIconValue: "",
                unit: MemoryUnits.GibiBytes);

        Assert.That(consoleOut.ToString(), Is.EqualTo("2 / 8"));
    }

    [Test]
    public async Task TestMemoryCommand_RespectsDefaultOptions()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetMemoryUsagePercent()).ReturnsAsync(75.0);
        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        });
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON",
            PercentIcon = "%"
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
                showIcon: true,
                iconValue: "",
                showPercentIcon: true,
                percentIconValue: "",
                unit: MemoryUnits.Percent);

        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON 75.00%"));
    }

    [Test]
    public async Task TestMemoryCommand_HandlesNullValues()
    {
        var mockStatsService = new Mock<ISystemStatsService>();
        var mockIconService = new Mock<IIconService>();

        mockStatsService.Setup(s => s.GetMemoryUsagePercent()).ReturnsAsync((double?)null);
        mockStatsService.Setup(s => s.GetMemoryUsageNumber(MemoryUnits.MibiBytes)).ReturnsAsync((string?)null);

        mockStatsService.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultMemoryUnit = MemoryUnits.MibiBytes
        });
        mockIconService.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            MemoryIcon = "MEM_ICON",
            PercentIcon = "%"
        });

        var cmd = new MemoryCommand(mockStatsService.Object, mockIconService.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        // Percent null
        await cmd.ExecuteAsync(
                showIcon: true,
                iconValue: "MEM_ICON",
                showPercentIcon: true,
                percentIconValue: "%",
                unit: MemoryUnits.Percent);
        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON %"));

        consoleOut.GetStringBuilder().Clear();

        // Number null
        await cmd.ExecuteAsync(
                showIcon: true,
                iconValue: "MEM_ICON",
                showPercentIcon: false,
                percentIconValue: "",
                unit: MemoryUnits.MibiBytes);
        Assert.That(consoleOut.ToString(), Is.EqualTo("MEM_ICON "));
    }
}


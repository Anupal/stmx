using Moq;
using stmx.Commands;
using stmx.Services;

namespace stmx.Tests;

public class CpuCommandTests
{
    [Test]
    public async Task TestCpuCommand_PrintsCustomIconAndPercent()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(37.42);

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: true,
            iconValue: "CPU_ICON",
            showPercentIcon: true,
            percentIconValue: "%"
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("CPU_ICON 37.42%"));
    }

    [Test]
    public async Task TestCpuCommand_PrintsPercentWithoutIcon()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(12.345);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());
        mockIcons.SetupGet(i => i.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON",
            PercentIcon = "%"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: false,
            iconValue: "",
            showPercentIcon: true,
            percentIconValue: ""
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("12.35%"));
    }

    [Test]
    public async Task TestCpuCommand_PrintsPercentWithoutPercentIcon()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(12.345);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());
        mockIcons.SetupGet(i => i.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON",
            PercentIcon = "%"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: false,
            iconValue: "",
            showPercentIcon: false,
            percentIconValue: ""
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("12.35"));
    }

    [Test]
    public async Task TestCpuCommand_RespectsDefaultOptions()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(37.42);
        mockIcons.SetupGet(i => i.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON",
            PercentIcon = "%"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: true,
            iconValue: "",
            showPercentIcon: true,
            percentIconValue: ""
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("CPU_ICON 37.42%"));
    }

    [Test]
    public async Task TestCpuCommand_HandlesNullCpuPercent()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync((double?)null);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: true,
            iconValue: "CPU_ICON",
            showPercentIcon: true,
            percentIconValue: "%"
        );


        Assert.That(consoleOut.ToString(), Is.EqualTo("CPU_ICON %"));
    }

    [Test]
    public async Task TestCpuCommand_FormatsToTwoDecimalPlaces()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(33.3333);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());
        mockIcons.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(
            showIcon: false,
            iconValue: "",
            showPercentIcon: true,
            percentIconValue: "%"
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("33.33%"));
    }
}

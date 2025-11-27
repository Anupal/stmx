using Moq;
using stmx.Commands;
using stmx.Services;

namespace stmx.Tests;

public class CpuCommandTests
{
    [Test]
    public async Task TestCpuCommand_PrintsIconAndPercent()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(37.42);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultShowMemoryIcon = true
        });
        mockIcons.SetupGet(i => i.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: true);

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
            CpuIcon = "CPU_ICON"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: false);

        Assert.That(consoleOut.ToString(), Is.EqualTo("12.35%"));
    }

    [Test]
    public async Task TestCpuCommand_RespectsDefaultOptions()
    {
        var opts = new SystemStatsServiceOptions
        {
            DefaultShowMemoryIcon = true
        };

        var mockStats = new Mock<ISystemStatsService>();
        mockStats.SetupGet(s => s.Options).Returns(opts);
        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(50.0);

        var mockIcons = new Mock<IIconService>();
        mockIcons.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: opts.DefaultShowMemoryIcon);

        Assert.That(consoleOut.ToString(), Is.EqualTo("CPU_ICON 50.00%"));
    }

    [Test]
    public async Task TestCpuCommand_HandlesNullCpuPercent()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync((double?)null);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions());
        mockIcons.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await cmd.ExecuteAsync(showIcon: true);

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

        await cmd.ExecuteAsync(showIcon: false);

        Assert.That(consoleOut.ToString(), Is.EqualTo("33.33%"));
    }

    [Test]
    public async Task TestCpuCommand_OptionParsingCallsExecute()
    {
        var mockStats = new Mock<ISystemStatsService>();
        var mockIcons = new Mock<IIconService>();

        mockStats.Setup(s => s.GetCpuUsagePercent()).ReturnsAsync(10.0);
        mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultShowMemoryIcon = false
        });
        mockIcons.SetupGet(s => s.Options).Returns(new IconServiceOptions
        {
            CpuIcon = "CPU_ICON"
        });

        var cmd = new CpuCommand(mockStats.Object, mockIcons.Object);

        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        // mimic command execution with icon enabled
        await cmd.ExecuteAsync(showIcon: true);

        Assert.That(consoleOut.ToString(), Is.EqualTo("CPU_ICON 10.00%"));
    }
}

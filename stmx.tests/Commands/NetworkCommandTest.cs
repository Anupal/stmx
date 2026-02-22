using Moq;
using stmx.Commands;
using stmx.Services;
using stmx.Utils;

namespace stmx.Tests;

public class NetworkCommandTests
{
    private Mock<ISystemStatsService> _mockStats;
    private Mock<IIconService> _mockIcons;
    private NetworkCommand _cmd;

    [SetUp]
    public void SetUp()
    {
        _mockStats = new Mock<ISystemStatsService>();
        _mockIcons = new Mock<IIconService>();

        _mockStats.SetupGet(s => s.Options).Returns(new SystemStatsServiceOptions
        {
            DefaultNetworkDirection = NetworkDirection.Both,
            DefaultNetworkUnit = NetworkUnits.Auto,
            DefaultNetworkDelay = 1
        });
        _mockIcons.SetupGet(i => i.Options).Returns(new IconServiceOptions
        {
            NetworkDownload = "DOWN_ICON",
            NetworkUpload = "UP_ICON"
        });

        _cmd = new NetworkCommand(_mockStats.Object, _mockIcons.Object);
    }

    private void SetupNetworkSpeed(double upload, double download, NetworkUnits unit = NetworkUnits.KiloBits)
    {
        _mockStats.Setup(s => s.GetNetworkSpeed(It.IsAny<NetworkUnits>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((
                new NetworkSpeed(upload, unit),
                new NetworkSpeed(download, unit)
            ));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsBothDirections()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Both,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("20.30k 10.50k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsDownloadOnly()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Download,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("20.30k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsUploadOnly()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Upload,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("10.50k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsDefaultDownloadIcon()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: true, downloadIconValue: "",
            direction: NetworkDirection.Download,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("DOWN_ICON 20.30k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsDefaultUploadIcon()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: true, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Upload,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("UP_ICON 10.50k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsCustomDownloadIcon()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: true, downloadIconValue: "↓",
            direction: NetworkDirection.Download,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("↓ 20.30k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsCustomUploadIcon()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: true, uploadIconValue: "↑",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Upload,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("↑ 10.50k"));
    }

    [Test]
    public async Task TestNetworkCommand_PrintsBothWithIcons()
    {
        SetupNetworkSpeed(10.5, 20.3);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: true, uploadIconValue: "↑",
            showDownloadIcon: true, downloadIconValue: "↓",
            direction: NetworkDirection.Both,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("↓ 20.30k ↑ 10.50k"));
    }

    [Test]
    public async Task TestNetworkCommand_FormatsToTwoDecimalPlaces()
    {
        SetupNetworkSpeed(10.1234, 20.5678);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Both,
            network: "wlo1", delay: 1, unit: NetworkUnits.KiloBits
        );

        Assert.That(consoleOut.ToString(), Is.EqualTo("20.57k 10.12k"));
    }

    [Test]
    public async Task TestNetworkCommand_UsesCorrectNetworkInterface()
    {
        SetupNetworkSpeed(0, 0);
        var consoleOut = new StringWriter();
        Console.SetOut(consoleOut);

        await _cmd.ExecuteAsync(
            showUploadIcon: false, uploadIconValue: "",
            showDownloadIcon: false, downloadIconValue: "",
            direction: NetworkDirection.Both,
            network: "eth0", delay: 1, unit: NetworkUnits.KiloBits
        );

        _mockStats.Verify(s => s.GetNetworkSpeed(It.IsAny<NetworkUnits>(), "eth0", It.IsAny<int>()), Times.Once);
    }
}

using Moq;

using stmx.Services;
using stmx.Utils;

namespace stmx.Tests;

public class LinuxSystemStatsServiceTests
{
    private Mock<IFileReader> _fileReaderMock = null!;
    private Mock<IFileSystem> _fileSystemMock = null!;
    private LinuxSystemStatsService _service = null!;


    [SetUp]
    public void Setup()
    {
        _fileReaderMock = new Mock<IFileReader>();
        _fileSystemMock = new Mock<IFileSystem>();

        _service = new LinuxSystemStatsService(_fileReaderMock.Object, _fileSystemMock.Object);

        _fileSystemMock.Setup(fs => fs.DirectoryExists("/sys/class/power_supply"))
                       .Returns(true);

        _fileSystemMock.Setup(fs => fs.GetDirectories("/sys/class/power_supply"))
                       .Returns(new[] { "/sys/class/power_supply/BAT0" });
    }

    [Test]
    public async Task TestGetBatteryCapacity_ReturnsParsedValue()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("85");

        var result = await _service.GetBatteryCapacity();

        Assert.That(result, Is.EqualTo(85));
    }

    [Test]
    public async Task TestGetBatteryCapacity_ReturnsNull_WhenIOException()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException());

        var result = await _service.GetBatteryCapacity();

        Assert.Throws<IOException>(() => _service.ReadBatteryCapacityFromSysFile());
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task TestGetBatteryStatus_Charging_ReturnsOneWhenCharging()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Charging");

        var result = await _service.GetBatteryStatus();

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task TestGetBatteryStatus_Charging_ReturnsOneWhenFull()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Full");

        var result = await _service.GetBatteryStatus();

        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task TestGetBatteryStatus_Discharging_ReturnsZero()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Discharging");

        var result = await _service.GetBatteryStatus();

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public async Task TestGetBatteryStatus_Unknown_ReturnsTwo()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("BadValue");

        var result = await _service.GetBatteryStatus();

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void TestGetBatterySysDirectory_ReturnsBAT1IfAvailable()
    {
        _fileSystemMock.Setup(fs => fs.GetDirectories("/sys/class/power_supply"))
                       .Returns(new[]
                       {
                           "/sys/class/power_supply/AC",
                           "/sys/class/power_supply/BAT1"
                       });

        var dir = _service.GetBatterySysDirectory();

        Assert.That(dir, Is.EqualTo("BAT1"));
    }

    [Test]
    public void TestGetBatterySysDirectory_ReturnsNullWhenNoBattery()
    {
        _fileSystemMock.Setup(fs => fs.GetDirectories("/sys/class/power_supply"))
                       .Returns(Array.Empty<string>());

        var dir = _service.GetBatterySysDirectory();

        Assert.That(dir, Is.Null);
    }

    [Test]
    public void ReadMemoryUsageFromProcFile_ParsesCorrectly()
    {
        string mockMemInfo = @"
MemTotal:       16384256 kB
MemAvailable:   12345678 kB
";

        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(mockMemInfo);

        var data = _service.ReadMemoryUsageFromProcFile();

        Assert.That(data.Total, Is.EqualTo(16384256));
        Assert.That(data.Available, Is.EqualTo(12345678));
        Assert.That(data.Used, Is.EqualTo(16384256 - 12345678));
        Assert.That(data.Unit, Is.EqualTo(MemoryUnits.KiloBytes));
    }

    [Test]
    public void MemoryUsageData_ConvertTo_ConvertsUnitsCorrectly()
    {
        var data = new MemoryUsageData(
            1_048_576,
            524_288,
            MemoryUnits.KiloBytes
        );

        var converted = data.ConvertTo(MemoryUnits.MibiBytes);

        Assert.That(converted.Total, Is.EqualTo(1000));
        Assert.That(converted.Available, Is.EqualTo(500));
        Assert.That(converted.Unit, Is.EqualTo(MemoryUnits.MibiBytes));
    }

    [Test]
    public async Task GetMemoryUsageNumber_ReturnsFormattedString()
    {
        string mockMemInfo = @"
MemTotal:       2097152 kB
MemAvailable:   1048576 kB
";

        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(mockMemInfo);

        string? result = await _service.GetMemoryUsageNumber(MemoryUnits.MibiBytes);

        Assert.That(result, Is.EqualTo("1000 / 2000"));
    }

    [Test]
    public async Task GetMemoryUsagePercent_ReturnsCorrectValue()
    {
        string mockMemInfo = @"
MemTotal:       2000 kB
MemAvailable:   500 kB
";

        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns(mockMemInfo);

        double? percent = await _service.GetMemoryUsagePercent(MemoryUnits.KiloBytes);

        Assert.That(percent, Is.EqualTo(75.0));
    }

    [Test]
    public async Task GetMemoryUsageNumber_ReturnsNull_OnIOException()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException());

        var result = await _service.GetMemoryUsageNumber(MemoryUnits.KiloBytes);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetMemoryUsagePercent_ReturnsNull_OnIOException()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException());

        var result = await _service.GetMemoryUsagePercent(MemoryUnits.KiloBytes);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ReadCpuUsageFromProcFile_ParsesCorrectly()
    {
        string mockCpuStat = "cpu  100 200 300 400 50 60 70 80 90 100\n";

        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>()))
            .Returns(mockCpuStat);

        var cpu = _service.ReadCpuUsageFromProcFile();

        Assert.That(cpu.User, Is.EqualTo(100));
        Assert.That(cpu.Nice, Is.EqualTo(200));
        Assert.That(cpu.System, Is.EqualTo(300));
        Assert.That(cpu.Idle, Is.EqualTo(400));
        Assert.That(cpu.IOWait, Is.EqualTo(50));
        Assert.That(cpu.IRQ, Is.EqualTo(60));
        Assert.That(cpu.SoftIRQ, Is.EqualTo(70));
        Assert.That(cpu.Steal, Is.EqualTo(80));
        Assert.That(cpu.Guest, Is.EqualTo(90));
        Assert.That(cpu.GuestNice, Is.EqualTo(100));
    }

    [Test]
    public async Task GetCpuUsagePercent_ComputesCorrectly()
    {
        // First sample
        string stat1 = "cpu  100 200 300 400 50 60 70 80 90 100\n";

        // Second sample slightly increased after delay
        string stat2 = "cpu  110 210 310 420 55 65 75 85 95 105\n";

        _fileReaderMock.SetupSequence(f => f.ReadAllText(It.IsAny<string>()))
            .Returns(stat1)
            .Returns(stat2);

        double? result = await _service.GetCpuUsagePercent();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.GreaterThan(0));
        Assert.That(result, Is.LessThanOrEqualTo(100));
    }

    [Test]
    public async Task GetCpuUsagePercent_ReturnsZero_WhenNoTimePasses()
    {
        // No difference between samples: totalDiff = 0
        string stat = "cpu  100 200 300 400 50 60 70 80 90 100\n";

        _fileReaderMock.SetupSequence(f => f.ReadAllText(It.IsAny<string>()))
            .Returns(stat)
            .Returns(stat);

        double? percent = await _service.GetCpuUsagePercent();

        Assert.That(percent, Is.EqualTo(0.0));
    }

    [Test]
    public async Task GetCpuUsagePercent_ReturnsNull_OnIOException()
    {
        _fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>()))
            .Throws(new IOException());

        var result = await _service.GetCpuUsagePercent();

        Assert.That(result, Is.Null);
    }
}


using Moq;

using stmx.Services;
using stmx.Utils;

namespace stmx.Tests
{
    public class LinuxSystemStatsServiceTests
    {
        private Mock<IFileReader> _fileReaderMock = null!;
        private LinuxSystemStatsService _service = null!;


        [SetUp]
        public void Setup()
        {
            _fileReaderMock = new Mock<IFileReader>();
            _service = new LinuxSystemStatsService(_fileReaderMock.Object);
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
    }
}


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
    }
}


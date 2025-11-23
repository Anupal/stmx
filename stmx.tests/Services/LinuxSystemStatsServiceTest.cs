using Moq;

using stmx.Services;
using stmx.Infrastructure;

namespace stmx.Tests
{
    public class LinuxSystemStatsServiceTests
    {
        [Test]
        public async Task TestGetBatteryCapacity_ReturnsParsedValue()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("85");

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryCapacity();

            Assert.That(result, Is.EqualTo(85));
        }

        [Test]
        public async Task TestGetBatteryCapacity_ReturnsNull_WhenIOException()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException());

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryCapacity();

            Assert.Throws<IOException>(() => service.ReadBatteryCapacityFromSysFile());
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task TestGetBatteryStatus_Charging_ReturnsOneWhenCharging()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Charging");

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryStatus();

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task TestGetBatteryStatus_Charging_ReturnsOneWhenFull()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Full");

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryStatus();

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task TestGetBatteryStatus_Discharging_ReturnsZero()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("Discharging");

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryStatus();

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task TestGetBatteryStatus_Unknown_ReturnsTwo()
        {
            var fileReaderMock = new Mock<IFileReader>();
            fileReaderMock.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("BadValue");

            var service = new LinuxSystemStatsService(fileReaderMock.Object);
            var result = await service.GetBatteryStatus();

            Assert.That(result, Is.EqualTo(2));
        }
    }
}


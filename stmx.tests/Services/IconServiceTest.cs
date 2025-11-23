
using stmx.Services;

namespace stmx.Tests
{
    public class IconServiceTests
    {
        [Test]
        public async Task TestGetBatteryCapacityIcon_UsesCorrectIndex()
        {
            var service = new IconService();
            var icon = await service.GetBatteryCapacityIcon(57);

            Assert.That(icon, Is.EqualTo(service.Options.BatteryCapacityIcons[5]));
        }

        [Test]
        public async Task TestGetBatteryStatusIcon_UsesCorrectIndex()
        {
            var service = new IconService();
            var icon = await service.GetBatteryStatusIcon(1);

            Assert.That(icon, Is.EqualTo(service.Options.BatteryStatusIcons[1]));
        }
    }
}

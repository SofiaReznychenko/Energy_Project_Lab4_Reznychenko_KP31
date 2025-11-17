using Xunit;
using Moq;
using Energy_Project.Services;
using Energy_Project.Services.Interfaces;
using Energy_Project.Models;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeTests
{
    public class EnergyMonitorServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo = new();
        private readonly Mock<IEnergyPlanRepository> _planRepo = new();
        private readonly Mock<INotificationService> _notifyService = new();
        private readonly EnergyMonitorService _energyMonitorService;

        public EnergyMonitorServiceTests()
        {
            _energyMonitorService = new EnergyMonitorService(_deviceRepo.Object, _planRepo.Object, _notifyService.Object);
        }

       
        [Fact]
        public void CalculateCurrentUsageKwh_ShouldReturnSumOfActiveDevicesPowerUsageInKwh()
        {
            var devices = new List<Device>
            {
                new Device { Id = 1, IsOn = true, PowerUsageWatts = 500 },
                new Device { Id = 2, IsOn = false, PowerUsageWatts = 300 },
                new Device { Id = 3, IsOn = true, PowerUsageWatts = 700 }
            };

            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);

            var result = _energyMonitorService.CalculateCurrentUsageKwh();

         
            Assert.Equal(1.2, result, 1); 
        }

       
        [Fact]
        public void CheckForOverload_ShouldSendAlert_WhenUsageExceedsLimit()
        {
            var devices = new List<Device>
            {
                new Device { Id = 1, IsOn = true, PowerUsageWatts = 1200 }
            };

            var plan = new EnergyPlan { DailyLimitKwh = 1.0 };

            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);
            _planRepo.Setup(p => p.GetCurrentPlan()).Returns(plan);

            _energyMonitorService.CheckForOverload();

            _notifyService.Verify(n => n.SendAlert(It.Is<string>(msg => msg.Contains("Overload detected"))), Times.Once);
        }

        
        [Fact]
        public void CheckForOverload_ShouldNotSendAlert_WhenUsageWithinLimit()
        {
            var devices = new List<Device>
            {
                new Device { Id = 1, IsOn = true, PowerUsageWatts = 500 }
            };

            var plan = new EnergyPlan { DailyLimitKwh = 1.0 };

            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);
            _planRepo.Setup(p => p.GetCurrentPlan()).Returns(plan);

            _energyMonitorService.CheckForOverload();

            _notifyService.Verify(n => n.SendAlert(It.IsAny<string>()), Times.Never);
        }

       
        [Fact]
        public void UpdateEnergyLimit_ShouldUpdatePlanDailyLimitAndCallUpdate()
        {
            var plan = new EnergyPlan { DailyLimitKwh = 1.0 };
            _planRepo.Setup(p => p.GetCurrentPlan()).Returns(plan);
            _planRepo.Setup(p => p.UpdatePlan(plan));

            _energyMonitorService.UpdateEnergyLimit(2.5);

            Assert.Equal(2.5, plan.DailyLimitKwh);
            _planRepo.Verify(p => p.UpdatePlan(plan), Times.Once);
        }
    }
}

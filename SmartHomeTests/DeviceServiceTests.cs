using Xunit;
using Moq;
using Energy_Project.Services;
using Energy_Project.Services.Interfaces;
using Energy_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SmartHomeTests
{
    public class DeviceServiceTests
    {
        private readonly Mock<IDeviceRepository> _deviceRepo = new();
        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _deviceService = new DeviceService(_deviceRepo.Object);
        }

        /// <summary>
        /// Тестування успішного увімкнення пристрою
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldTurnOnDevice_WhenDeviceExists()
        {
            var device = new Device { Id = 1, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(1)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            var result = _deviceService.ToggleDevice(1, true);

            Assert.True(result);
            Assert.True(device.IsOn);

            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.Id == 1 && d.IsOn == true)), Times.Once);
        }

        /// <summary>
        /// Тестування винятку, коли пристрій не знайдено
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldThrowArgumentException_WhenDeviceNotFound()
        {
            _deviceRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Device?)null);

            Assert.Throws<ArgumentException>(() => _deviceService.ToggleDevice(999, true));
            _deviceRepo.Verify(r => r.Update(It.IsAny<Device>()), Times.Never);
        }

        /// <summary>
        /// Параметризований тест для перевірки встановлення стану пристрою
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ToggleDevice_ShouldSetDeviceStateAccordingToParameter(bool turnOn)
        {
            var device = new Device { Id = 2, IsOn = !turnOn };
            _deviceRepo.Setup(r => r.GetById(2)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            var result = _deviceService.ToggleDevice(2, turnOn);

            Assert.Equal(turnOn, result);
            Assert.Equal(turnOn, device.IsOn);

            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.Id == 2 && d.IsOn == turnOn)), Times.Once);
        }

        /// <summary>
        /// Перевірка, що GetActiveDevices повертає тільки увімкнені пристрої
        /// </summary>
        [Fact]
        public void GetActiveDevices_ShouldReturnOnlyDevicesThatAreOn()
        {
            var devices = new List<Device>
            {
                new Device { Id = 1, IsOn = true },
                new Device { Id = 2, IsOn = false },
                new Device { Id = 3, IsOn = true }
            };
            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);

            var activeDevices = _deviceService.GetActiveDevices().ToList();

            Assert.NotEmpty(activeDevices);
            Assert.DoesNotContain(activeDevices, d => !d.IsOn);
            Assert.Equal(2, activeDevices.Count);
        }

        /// <summary>
        /// Перевірка, що GetActiveDevices повертає пустий список, якщо нема активних пристроїв
        /// </summary>
        [Fact]
        public void GetActiveDevices_ShouldReturnEmpty_WhenNoDevicesAreOn()
        {
            var devices = new List<Device>
            {
                new Device { Id = 1, IsOn = false },
                new Device { Id = 2, IsOn = false }
            };
            _deviceRepo.Setup(r => r.GetAll()).Returns(devices);

            var activeDevices = _deviceService.GetActiveDevices();

            Assert.Empty(activeDevices);
        }

        /// <summary>
        /// Перевірка що ToggleDevice викликає метод Update репозиторію один раз
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldCallUpdateOnce()
        {
            var device = new Device { Id = 3, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(3)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            _deviceService.ToggleDevice(3, true);

            _deviceRepo.Verify(r => r.Update(It.IsAny<Device>()), Times.Once);
        }

        /// <summary>
        /// Перевірка, що ToggleDevice не викликає Update, якщо пристрій не знайдено
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldNotCallUpdate_WhenDeviceNotFound()
        {
            _deviceRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns((Device?)null);

            Assert.Throws<ArgumentException>(() => _deviceService.ToggleDevice(10, true));

            _deviceRepo.Verify(r => r.Update(It.IsAny<Device>()), Times.Never);
        }

        /// <summary>
        /// Перевірка що ToggleDevice встановлює правильний параметр IsOn у переданому пристрої
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldSetCorrectIsOnValue()
        {
            var device = new Device { Id = 4, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(4)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            _deviceService.ToggleDevice(4, true);

            Assert.True(device.IsOn);
        }

        /// <summary>
        /// Перевірка що ToggleDevice повертає false, якщо вимикаємо пристрій
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldReturnFalse_WhenTurnOff()
        {
            var device = new Device { Id = 5, IsOn = true };
            _deviceRepo.Setup(r => r.GetById(5)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            var result = _deviceService.ToggleDevice(5, false);

            Assert.False(result);
        }

        /// <summary>
        /// Перевірка що ToggleDevice приймає будь-яке значення id
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldAcceptAnyId()
        {
            var device = new Device { Id = 123, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(It.IsAny<int>())).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            var result = _deviceService.ToggleDevice(123, true);

            Assert.True(result);
            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.Id == 123 && d.IsOn == true)), Times.Once);
        }

        /// <summary>
        /// Перевірка що ToggleDevice викликає Update із Device, у якого IsOn задано true
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldCallUpdateWithIsOnTrue()
        {
            var device = new Device { Id = 6, IsOn = false };
            _deviceRepo.Setup(r => r.GetById(6)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            _deviceService.ToggleDevice(6, true);

            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.IsOn == true)), Times.Once);
        }

        /// <summary>
        /// Перевірка що ToggleDevice викликає Update із Device, IsOn задано false
        /// </summary>
        [Fact]
        public void ToggleDevice_ShouldCallUpdateWithIsOnFalse()
        {
            var device = new Device { Id = 7, IsOn = true };
            _deviceRepo.Setup(r => r.GetById(7)).Returns(device);
            _deviceRepo.Setup(r => r.Update(It.IsAny<Device>()));

            _deviceService.ToggleDevice(7, false);

            _deviceRepo.Verify(r => r.Update(It.Is<Device>(d => d.IsOn == false)), Times.Once);
        }
        //--- NotEqual----//
        [Fact]
public void ToggleDevice_ShouldChangeState()
{
    var device = new Device { Id = 8, IsOn = false };
    _deviceRepo.Setup(r => r.GetById(8)).Returns(device);

    _deviceService.ToggleDevice(8, true);

    Assert.NotEqual(false, device.IsOn);
}
    //--- NotNull ---//
    [Fact]
public void GetActiveDevices_ShouldNotReturnNull()
{
    _deviceRepo.Setup(r => r.GetAll()).Returns(new List<Device>());

    var result = _deviceService.GetActiveDevices();

    Assert.NotNull(result);
}

    }
}

﻿using System;
using System.Linq;
using Buttplug.Server.Bluetooth;
using Buttplug.Core;
using Xunit;

namespace Buttplug.Server.Test
{
    public class ButtplugDeviceTests
    {
        [Fact]
        public void TestBuiltinDeviceLoading()
        {
            var buttplugAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(aAssembly => aAssembly.GetName().Name == "Buttplug.Server");
            Assert.NotNull(buttplugAssembly);
            var types = buttplugAssembly.GetTypes()
                .Where(aType => aType.IsClass && aType.Namespace == "Buttplug.Server.Bluetooth.Devices" &&
                            typeof(IBluetoothDeviceInfo).IsAssignableFrom(aType)).ToList();
            Assert.True(types.Any());
            var b = new TestBluetoothSubtypeManager(new ButtplugLogManager());
            var d = b.GetDefaultDeviceInfoList();
            foreach (var t in types)
            {
                Assert.True(d.Any(aInfoObj => aInfoObj.GetType() == t), $"Default types contains type: {t.Name}");
            }
        }
    }
}

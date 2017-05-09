﻿using Buttplug.Core;
using Buttplug.Messages;
using Xunit;

namespace ButtplugTest.Core
{
    public class ButtplugServerTests
    {
        [Fact]
        public async void RejectOutgoingOnlyMessage()
        {
            Assert.True((await new ButtplugService().SendMessage(new Error("Error", ButtplugConsts.DEFAULT_MSG_ID))).IsLeft);
        }

        [Fact]
        public async void LoggerSettingsTest()
        {
            var gotMessage = false;
            var s = new ButtplugService();
            s.MessageReceived += (obj, msg) =>
            {
                if (msg.Message.GetType() == typeof(Log))
                {
                    gotMessage = true;
                }
            };
            // Sending error messages will always cause an error, as they are outgoing, not incoming.
            await s.SendMessage(new Error("Error", ButtplugConsts.DEFAULT_MSG_ID));
            Assert.False(gotMessage);
            Assert.True((await s.SendMessage(new RequestLog("Trace"))).IsRight);
            Assert.True((await s.SendMessage(new Error("Error", ButtplugConsts.DEFAULT_MSG_ID))).IsLeft);
            Assert.True(gotMessage);
            await s.SendMessage(new RequestLog("Off"));
            gotMessage = false;
            await s.SendMessage(new Error("Error", ButtplugConsts.DEFAULT_MSG_ID));
            Assert.False(gotMessage);
        }

        [Fact]
        public async void CheckMessageReturnId()
        {
            var s = new ButtplugService();
            s.MessageReceived += (obj, msg) =>
            {
                Assert.True(msg.Message is RequestServerInfo);
                Assert.True(msg.Message.Id == 12345);
            };
            var m = new RequestServerInfo(12345);
            await s.SendMessage(m);
            await s.SendMessage("{\"RequestServerInfo\":{\"Id\":12345}}");
        }

        [Fact]
        public async void AddDeviceTest()
        {
            var d = new TestDevice("TestDevice");
            var m = new TestDeviceManager(d);
            var s = new TestService(m);
            var msgReceived = false;
            s.MessageReceived += (obj, msgArgs) =>
            {
                msgReceived = true;
                switch (msgArgs.Message)
                {
                    case DeviceAdded da:
                        Assert.True(da.DeviceName == "TestDevice");
                        Assert.True(da.DeviceIndex == 0);
                        break;

                    default:
                        Assert.False(msgArgs.Message is DeviceAdded);
                        break;
                }
            };
            Assert.True((await s.SendMessage(new StartScanning(1))).IsRight);
            Assert.True(msgReceived);
        }
    }
}
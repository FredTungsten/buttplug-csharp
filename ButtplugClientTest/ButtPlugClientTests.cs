using System;
using System.Threading;
using Buttplug.Core;
using Buttplug.Messages;
using ButtplugClient.Core;
using ButtplugWebsockets;
using Xunit;

namespace ButtplugClientTest
{
    public class ButtplugClientTests : IButtplugServiceFactory
    {
        public ButtplugService GetService()
        {
            return new ButtplugService("Test service", 0);
        }

        [Fact]
        public void TestConnection()
        {
            Console.WriteLine("starting server");
            var server = new ButtplugWebsocketServer();
            server.StartServer(this);

            Console.WriteLine("connecting");
            var client = new ButtplugWSClient("Test client");
            client.Connect(new Uri("ws://localhost:12345")).Wait();

            Console.WriteLine("test msg 1");
            var msgId = client.nextMsgId;
            var res = client.SendMessage(new Test("Test string", msgId)).GetAwaiter().GetResult();
            Assert.True(res != null);
            Assert.True(res is Test);
            Assert.True(((Test)res).TestString == "Test string");
            Assert.True(((Test)res).Id == msgId);

            // Check ping is working
            Thread.Sleep(200);

            Console.WriteLine("test msg 2");
            msgId = client.nextMsgId;
            res = client.SendMessage(new Test("Test string", msgId)).GetAwaiter().GetResult();
            Assert.True(res != null);
            Assert.True(res is Test);
            Assert.True(((Test)res).TestString == "Test string");
            Assert.True(((Test)res).Id == msgId);

            Console.WriteLine("FINISHED CLIENT DISCONNECT");
            // Shut it down
            client.Diconnect().Wait();
            Console.WriteLine("FINISHED SERVER STOP");
            server.StopServer();
        }
    }
}

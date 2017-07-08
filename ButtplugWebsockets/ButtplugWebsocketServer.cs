﻿using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buttplug.Core;
using JetBrains.Annotations;
using vtortola.WebSockets;
// using HttpStatusCode = WebSocketSharp.Net.HttpStatusCode;

namespace ButtplugWebsockets
{
    public class ButtplugWebsocketServer
    {
        private WebSocketListener _server;

        public void StartServer([NotNull] IButtplugServiceFactory aFactory, int aPort = 12345, bool aSecure = false)
        {
            CancellationTokenSource cancellation = new CancellationTokenSource();
            Console.WriteLine("Starting server");
            var endpoint = new IPEndPoint(IPAddress.Loopback, aPort);
            var _server = new WebSocketListener(endpoint);
            var rfc6455 = new vtortola.WebSockets.Rfc6455.WebSocketFactoryRfc6455(_server);
            _server.Standards.RegisterStandard(rfc6455);
            _server.Start();
            Console.WriteLine("Server started.");
            var task = Task.Run(() => AcceptWebSocketClientsAsync(_server, cancellation.Token));
        }

        private static async Task AcceptWebSocketClientsAsync(WebSocketListener server, CancellationToken token)
        {
            Console.WriteLine("Got connection!");
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var ws = await server.AcceptWebSocketAsync(token).ConfigureAwait(false);
                    Console.WriteLine("accepting");
                    if (ws != null)
                    {
                       Task.Run(() => HandleConnectionAsync(ws, token));
                    }
                }
                catch (Exception aex)
                {
                    // Log("Error Accepting clients: " + aex.GetBaseException().Message);
                }
            }
            // Log("Server Stop accepting clients");
        }

        private static async Task HandleConnectionAsync(WebSocket ws, CancellationToken cancellation)
        {
            Console.WriteLine("Listening");
            var buttplug = new ButtplugService("Websocket Server", 0);
            buttplug.MessageReceived += async (aObject, aEvent) =>
            {
                if (buttplug == null)
                {
                    return;
                }
                var msg = buttplug.Serialize(aEvent.Message);
                await ws.WriteStringAsync(msg, cancellation);
            };

            try
            {
                while (ws.IsConnected && !cancellation.IsCancellationRequested)
                {
                    var msg = await ws.ReadStringAsync(cancellation).ConfigureAwait(false);
                    Console.WriteLine("Got message!");
                    if (msg != null)
                    {
                        var respMsg = buttplug.Serialize(await buttplug.SendMessage(msg));
                        await ws.WriteStringAsync(respMsg, cancellation);
                    }
                }
            }
            catch (Exception aex)
            {
                // Log("Error Handling connection: " + aex.GetBaseException().Message);
                try { ws.Close(); }
                catch { }
            }
            finally
            {
                buttplug = null;
                ws.Dispose();
            }
        }

        public void StopServer()
        {
            if (_server != null)
            {
                _server.Stop();
            }
        }
    }
}

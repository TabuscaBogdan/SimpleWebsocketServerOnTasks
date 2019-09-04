using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WSChatApp
{
    public class Server
    {
        public Dictionary<int, WebSocket> Connections;
        public int ClientNumber = 0;

        private readonly HttpListener httpListener;
        private readonly Mutex signal;
        public Server(string endpoint)
        {
            httpListener = new HttpListener();
            signal = new Mutex();
            httpListener.Prefixes.Add(endpoint);

            Connections = new Dictionary<int, WebSocket>();
        }

        public void Start()
        {
            httpListener.Start();
            while (signal.WaitOne())
            {
                Task.Run(async () => await ReceiveConnection().ConfigureAwait(false));
            }
        }

        private async Task ReceiveConnection()
        {
            var context = await httpListener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                int id;
                lock (Connections)
                {
                    Interlocked.Increment(ref ClientNumber);
                    id = ClientNumber;
                    Connections[ClientNumber] = webSocket;
                }

                await HandleConnection(webSocket, id);
            }

            signal.ReleaseMutex();
        }

        private async Task HandleConnection(WebSocket webSocket,int id)
        {
            var receivedBytes = new byte[1024];

            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Connected.")),
                WebSocketMessageType.Text, true, CancellationToken.None);

            while (webSocket.State == WebSocketState.Open)
            {
                await webSocket.ReceiveAsync(new ArraySegment<byte>(receivedBytes), CancellationToken.None);

                var message = Encoding.Default.GetString(receivedBytes);
                message = message.Replace("\0", string.Empty);

                foreach (var connectionId in Connections.Keys)
                {
                    await Connections[connectionId].SendAsync(
                        new ArraySegment<byte>(Encoding.UTF8.GetBytes(id+" "+message),0,message.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }

            }
        }
    }
}

using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChatServer
{
    partial class Application
    {
        internal class ClientHandler
        {
            private const int RECV_BUFFER_SIZE = 1024 * 4;

            private readonly Application.AppServer _server;
            private readonly HttpContext _context;
            private readonly WebSocket _webSocket;

            public ClientHandler(Application.AppServer server, HttpContext context, WebSocket webSocket)
            {
                this._server = server ?? throw new ArgumentNullException(nameof(server));
                this._context = context ?? throw new ArgumentNullException(nameof(context));
                this._webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            }

            public User ChatUser { get; private set; }

            public async Task ProcessClientSocket()
            {
                ChatUser = _server.AddClient(_context, _webSocket);

                Console.WriteLine($"{ChatUser} is connected", ChatUser);
                await SendMessageToAll($"{ChatUser} is connected", ChatUser.ToString()); // send message to all, including self

                await ReceiveMessage();

                _server.RemoveClient(ChatUser.Id);

                Console.WriteLine($"{ChatUser} is disconnected", ChatUser);
                await SendMessageToAll($"{ChatUser} is disconnected", ChatUser.ToString());
            }

            private async Task ReceiveMessage()
            {
                var buffer = new byte[RECV_BUFFER_SIZE];

                WebSocketReceiveResult result;
                while (true)
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (!result.CloseStatus.HasValue)
                    {
                        string message = $"{ChatUser.Name}: {System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count)}";
                        await SendMessageToAll(message, ChatUser.Id);
                    }
                    else
                    {
                        break;
                    }
                }

                await _webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }

            private async Task SendMessageToAll(string message, string myConnectionId)
            {
                byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                foreach (var pair in _server.ChatConnections)
                {
                    if (pair.Key != myConnectionId && pair.Value.State == WebSocketState.Open)
                    {
                        await pair.Value.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }

#pragma warning disable IDE0051 // Remove unused private members
            private async Task SendMessage(string message, string otherConnectionId)
#pragma warning restore IDE0051 // Remove unused private members
            {
                if (_server.ChatConnections.ContainsKey(otherConnectionId))
                {
                    if (_server.ChatConnections[otherConnectionId].State == WebSocketState.Open)
                    {
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                        await _server.ChatConnections[otherConnectionId].SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    Console.WriteLine($"{otherConnectionId} not found in the dictionary", otherConnectionId);
                }
            }
        }
    }
}

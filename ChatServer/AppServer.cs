using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ChatServer
{
    partial class Application
    {
        public class AppServer
        {
            public ConcurrentDictionary<string, WebSocket> ChatConnections { get; private set; }
            public ConcurrentDictionary<string, User> ChatUsers { get; private set; }

            public override string ToString()
            {
                return base.ToString();
            }

            public void Configure(IApplicationBuilder app)
            {
                ChatConnections = new ConcurrentDictionary<string, WebSocket>();
                ChatUsers = new ConcurrentDictionary<string, User>();

                var webSocketOptions = new WebSocketOptions
                {
                    KeepAliveInterval = TimeSpan.FromMinutes(2)
                };

                app.UseWebSockets(webSocketOptions);
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/ws")
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            await new ClientHandler(this, context, webSocket).ProcessClientSocket();
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }
                    }
                    else
                    {
                        await next();
                    }
                });
            }

            internal void RemoveClient(string connectionId)
            {
                ChatUsers.TryRemove(connectionId, out _);
                ChatConnections.TryRemove(connectionId, out _);
            }

            internal User AddClient(HttpContext context, WebSocket webSocket)
            {
                string connectionId = Guid.NewGuid().ToString();
                ChatConnections.TryAdd(connectionId, webSocket);

                User chatUser = new User(connectionId,
                    context.Request.Query["name"],
                    context.Request.Query["email"]);

                ChatUsers.TryAdd(connectionId, chatUser);
                return chatUser;
            }
        }
    }
}

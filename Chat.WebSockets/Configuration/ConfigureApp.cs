using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Chat.WebSockets.Configuration
{
    public static class ConfigureApp
    {
        // Armazenamento das conexões ativas
        private static ConcurrentDictionary<string, WebSocket> _connectedClients = new ConcurrentDictionary<string, WebSocket>();

        public static void ConfigureWebSockets(this WebApplication app)
        {
            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var socket = await context.WebSockets.AcceptWebSocketAsync();
                    var clientId = context.Connection.Id;

                    _connectedClients.TryAdd(clientId, socket);

                    await HandleWebSocketCommunication(clientId, socket);
                }
                else
                {
                    await next();
                }
            });

        }


        private static async Task HandleWebSocketCommunication(string clientId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                // Recebe a mensagem do cliente
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _connectedClients.TryRemove(clientId, out _);
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed by client", CancellationToken.None);
                    break;
                }

                // Transforma os dados recebidos em string
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var responseMessage = $"{message} at {DateTime.Now}";

                // Envia a mensagem recebida para todos os clientes conectados
                await SendMessageToAllClients(responseMessage);
            }
        }

        private static async Task SendMessageToAllClients(string message)
        {
            foreach (var client in _connectedClients)
            {
                if (client.Value.State == WebSocketState.Open)
                {
                    var messageBuffer = Encoding.UTF8.GetBytes(message);
                    await client.Value.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}

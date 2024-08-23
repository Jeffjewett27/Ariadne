using UnityEngine;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ariadne.Socket
{

    public class WebSocketServerManager
    {
        private static WebSocketServerManager instance;

        private readonly WebSocketServer wss;
        private readonly List<SocketServerBehavior> clients = new();
        private readonly int port;
        private bool isOpen;

        private WebSocketServerManager(int port)
        {
            this.port = port;
            this.isOpen = false;

            // Create the WebSocket server
            wss = new WebSocketServer($"ws://localhost:{port}");

            // Add a WebSocket behavior to handle connections
            wss.AddWebSocketService<SocketServerBehavior>("/");
        }

        public static WebSocketServerManager Instance
        {
            get
            {
                instance ??= new WebSocketServerManager(8645);
                return instance;
            }
        }

        public void Open()
        {
            if (isOpen) return;

            // Start the server
            wss.Start();
            isOpen = true;

            Ariadne.MLog($"WebSocket server started on ws://localhost:{port}");
        }

        public void Close()
        {
            wss?.Stop();
            instance = null;
        }

        public void RegisterClient(SocketServerBehavior client)
        {
            clients.Add(client);
        }

        public void UnregisterClient(SocketServerBehavior client)
        {
            clients.Remove(client);
        }

        public void BroadcastMessage(string message)
        {
            Task.Run(() => BroadcastMessageAsync(message));
        }

        private async Task BroadcastMessageAsync(string message)
        {
            List<Task> sendTasks = new();
            foreach (var client in clients)
            {
            //Ariadne.MLog($"Broadcasting message async: {message}");
                if (client != null && client.Context.WebSocket.IsAlive)
                {
                    sendTasks.Add(Task.Run(() => client.SendMessage(message)));
                }
            }

            await Task.WhenAll(sendTasks);
        }
    }

}

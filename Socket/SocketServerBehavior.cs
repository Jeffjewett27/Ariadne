using WebSocketSharp;
using WebSocketSharp.Server;

namespace Ariadne.Socket
{
    public class SocketServerBehavior : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            WebSocketServerManager.Instance.RegisterClient(this);
            Ariadne.MLog("Client connected");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            WebSocketServerManager.Instance.UnregisterClient(this);
            Ariadne.MLog("Client disconnected");
        }

        public void SendMessage(string message)
        {
            Send(message);
        }
    }
}

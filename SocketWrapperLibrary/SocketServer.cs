using System.Net;
using System.Net.Sockets;

namespace SocketWrapperLibrary
{
    public class SocketServer
    {
        private Socket? serverSocket;
        internal List<ClientHandler> socketConnections = new List<ClientHandler>();
        private Thread? socketListenerThread;

        public void Start(int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

            serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);

            socketListenerThread = new Thread(SocketListenerThreadFunc);
            socketListenerThread.Start();
        }

        private void SocketListenerThreadFunc()
        {
            while (serverSocket != null && serverSocket.IsBound)
            {
                serverSocket.Listen(0);
                socketConnections.Add(new ClientHandler(serverSocket.Accept(), this));
            }
        }

        public void DisconnectAllClients()
        {
            foreach (ClientHandler client in socketConnections)
            {
                client.StopClientHandler();
                socketConnections.Remove(client);
            }
        }

        public void StopServer()
        {
            if (serverSocket == null) return;

            serverSocket.Close();
            serverSocket = null;
        }
    }

    internal class ClientHandler : SocketClient
    {
        SocketServer server;
        Thread dataReceiverThread;

        public ClientHandler(Socket socket, SocketServer server)
        {
            this.socket = socket;
            this.server = server;

            dataReceiverThread = new Thread(ReceiveDataThreadFunc);
            dataReceiverThread.Start();
        }

        public void ReceiveDataThreadFunc()
        {
            while (socket != null && socket.Connected)
            {
                SendDataToAllClients(ReceiveData()); // SendDataToAllClients could be defined as async? Something tells me this could improve performance later.
            }
        }

        public void SendDataToAllClients(byte[] data)
        {
            foreach (ClientHandler client in server.socketConnections)
            {
                if (client == this) continue;
                client.SendData(data);
            }
        }

        public void StopClientHandler()
        {
            if (socket == null) return;

            // Thread method will detect the abscence of a socket I think, so no handling here for now
            if (socket.Connected) socket.Close();
            socket = null;
        }
    }
}

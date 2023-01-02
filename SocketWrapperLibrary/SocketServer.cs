using System.Net;
using System.Net.Sockets;

namespace SocketWrapperLibrary
{
    class SocketServer
    {
        private static Socket serverSocket;
        internal static List<ClientHandler> socketConnections= new List<ClientHandler>();

        public static void Start(int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);
            
            while (serverSocket != null && serverSocket.IsBound)
            {
                serverSocket.Listen(0);
                socketConnections.Add(new ClientHandler(serverSocket.Accept()));
            }
        }
    }

    internal class ClientHandler : SocketClient // Create a "SocketElements" class to provide the send and receive methods? (maybe not, but this inheritance is not good either)
    {
        Thread dataReceiverThread;

        public ClientHandler(Socket socket)
        {
            this.socket = socket;
        }

        

    }
}

using System.Net;
using System.Net.Sockets;

namespace SocketWrapperLibrary
{
    public class SocketServer
    {
        private Socket? serverSocket;
        internal List<ClientHandler> SocketConnections { get; private set; } = new List<ClientHandler>();
        private Thread? socketListenerThread;

        public void Start(int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            serverSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipEndPoint);

            socketListenerThread = new Thread(SocketListenerThreadFunc);
            socketListenerThread.Start();
        }

        private void SocketListenerThreadFunc()
        {
            try
            {
                while (serverSocket != null && serverSocket.IsBound)
                {
                    serverSocket.Listen(0);
                    SocketConnections.Add(new ClientHandler(serverSocket.Accept(), this)); // This always throw an exception when closing the server window.
                }
            } catch (Exception e)
            {
                Console.WriteLine($"ERROR (SocketListenerThreadFunc): {e.Message}\n... Closing server connection.");
            } finally
            {
                StopServer();
            }
            
        }

        internal void DisconnectClient(ClientHandler client)
        {
            client.StopClientHandler();
            SocketConnections.Remove(client);
        }

        public void DisconnectAllClients()
        {
            // Fixing a bug where the list is modified while foreach's running - Now it access the items directly
            for (int i = 0; i < SocketConnections.Count; i++)
            {
                DisconnectClient(SocketConnections[i]);
            }

            SocketConnections.Clear();
        }

        public void StopServer()
        {
            if (serverSocket == null) return;

            DisconnectAllClients();
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

        public void SendDataToAllClients(byte[] data) // Not static I think
        {
            // Fixing a bug where the list is modified while foreach's running - Now it access the items directly
            for (int i = 0; i < server.SocketConnections.Count; i++)
            {
                ClientHandler client = server.SocketConnections[i];

                if (client == this) continue;
                if (!client.SendData(data))
                {
                    Console.WriteLine("Could not 'relay' message from a client to the rest.");
                    if (!client.IsConnected())
                    {
                        server.DisconnectClient(client);
                        Console.WriteLine("Removed client for being disconnected.");
                    }                    
                }
            }
        }

        public void StopClientHandler()
        {
            if (socket == null) return;

            // ATTENTION: Do *NOT* call server.DisconnectClient() here. Result: Stack Overflow  O_O

            // Thread method will detect the abscence of a socket I think, so no handling here for now

            if (socket.Connected) socket.Close();
            socket = null;
        }
    }
}

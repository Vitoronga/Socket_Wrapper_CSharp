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

        public void SendDataAsServer(byte[] bytes)
        {
            for (int i = 0; i < SocketConnections.Count; i++)
            {
                SocketConnections[i].SendData(bytes);
            }
        }

        public string GetClientsStatus()
        {
            string msgOut = $"All Clients Status ({SocketConnections.Count}):";

            for (int i = 0; i < SocketConnections.Count; i++)
            {
                msgOut += $"\n{i}: " + SocketConnections[i].GetClientStatus();
            }

            return msgOut;
        }

        public string GetClientReportFromId(int id)
        {
            if (!TryFindClientHandlerFromId(id, out ClientHandler client)) return $"Couldn't find client from id {id}";
            return client.GetClientReport();
        }

        public bool DisconnectClientById(int id)
        {
            if (!TryFindClientHandlerFromId(id, out ClientHandler client)) return false;
            DisconnectClient(client);

            return true;
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

        public bool IsServerOnline() => serverSocket != null && serverSocket.IsBound;

        public void StopServer()
        {
            Console.WriteLine("Closing server..."); // For debug reasons

            if (serverSocket == null) return;

            DisconnectAllClients();
            serverSocket.Close();
            serverSocket = null;

            Console.WriteLine("Closing process done."); // For debug reasons
        }

        internal bool TryFindClientHandlerFromId(int id, out ClientHandler? client)
        {
            for (int i = 0; i < SocketConnections.Count; i++)
            {
                if (SocketConnections[i].Id == id)
                {
                    client = SocketConnections[i];
                    return true;
                }
            }

            client = null; // IDE shows a warning if 'out ClientHandler' here isn't nullable
            return false;
        }
    }

    internal class ClientHandler : SocketClient
    {
        SocketServer server;
        Thread dataReceiverThread;
        internal static int ClientCount { get; private set; } // Useful to generate unique IDs
        internal int Id { get; private set; }
        internal DateTime ConnectionTime { get; private set; }
        private int socketEmissionToServerCount = 0; // Socket communication tracker
        private int socketEmissionToClientCount = 0; // Socket communication tracker

        public ClientHandler(Socket socket, SocketServer server)
        {
            this.socket = socket;
            this.server = server;
            this.Id = ClientCount++;
            ConnectionTime = DateTime.Now;

            dataReceiverThread = new Thread(ReceiveDataThreadFunc);
            dataReceiverThread.Start();
        }

        public void ReceiveDataThreadFunc()
        {
            while (socket != null && socket.Connected)
            {
                if (!ReceiveData(out byte[] bytes)) continue;
                socketEmissionToServerCount++;
                SendDataToAllClients(bytes); // SendDataToAllClients could be defined as async? Something tells me this could improve performance later.
            }
        }

        public new bool SendData(byte[] bytes) // The only difference between this and the base one is the tracker.
        {
            bool success = base.SendData(bytes);
            
            if (!success) return false;
            
            socketEmissionToClientCount++;
            return true;
        }

        public void SendDataToAllClients(byte[] data)
        {
            // Fixing a bug where the list is modified while foreach's running - Now it access the items directly
            for (int i = 0; i < server.SocketConnections.Count; i++)
            {
                ClientHandler client = server.SocketConnections[i];

                // Ignore self
                if (client == this) continue;
                
                if (!client.SendData(data))
                {
                    Console.WriteLine("Could not 'relay' message from a client to another.");
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

        public string GetClientStatus() // Simple status return
        {
            string msgOut = "Id: " + Id +
                          "\nConnected: " + IsConnected() +
                          "\nTime connected: " + GetTimeConnected() + "s";

            return msgOut;
        }

        public string GetClientReport() // Detailed status return
        {
            string msgOut = GetClientStatus();

            msgOut += "\nConnection Time: " + ConnectionTime + 
                      "\nEmissions (C->S): " + socketEmissionToServerCount +
                      "\nReceptions (S->C): " + socketEmissionToClientCount;

            return msgOut;
        }

        public long GetTimeConnected()
        {
            return ((DateTime.Now.Ticks - ConnectionTime.Ticks) / TimeSpan.TicksPerSecond);
        }
    }
}

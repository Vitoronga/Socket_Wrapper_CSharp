using SocketWrapperLibrary;
using System.Text;

namespace SocketWrapperImplementation
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start as client (C) or server (S)?");
            string input = Console.ReadLine();
            if (input == "S")
            {
                ServerHandler();
            } else
            {
                ClientHandler();
            }
        }

        private static void ServerHandler()
        {
            string input = "";

            SocketServer server = new SocketServer();
            server.Start(20100);

            Console.WriteLine("Press enter to stop server."); // Can change later for debugging other features, like disconnect all client...
            Console.ReadLine();
            server.StopServer();
        }

        private static void ClientHandler()
        {
            string input = "";

            SocketClient client = new SocketClient();
            client.Connect("127.0.0.1", 20100);

            Console.WriteLine("Configuring client. Type S for sender or R for receiver");
            input = Console.ReadLine();

            if (input.ToUpper().Equals("S"))
            {
                input = "Test";
                while (input != "Q" && client.IsConnected())
                {
                    bool success = client.SendData(Encoding.ASCII.GetBytes(input));
                    if (!success) Console.WriteLine("<<< Failed to send data >>>");
                    Console.Write(">>> ");
                    input = Console.ReadLine();
                }
            }
            else
            {
                while (client.IsConnected())
                {
                    Console.WriteLine(Encoding.ASCII.GetString(client.ReceiveData()));
                }
            }

            client.CloseConnection();
        }

    }
}
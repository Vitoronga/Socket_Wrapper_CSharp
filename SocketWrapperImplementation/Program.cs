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
            //server.Start(20100); // Done in the menu

            //Console.WriteLine("Press enter to stop server."); // Can change later for debugging other features, like disconnect all client...
            //Console.ReadLine();

            while (input != "10")
            {
                PrintServerMenu(server.IsServerOnline());

                input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        server.Start(20100);
                        break;


                    case "2":
                        Console.WriteLine($"Clients: {server.GetClientsStatus()}");
                        break;


                    case "3":
                        Console.WriteLine("Type the id of the client you want to inspect:");
                        if (!int.TryParse(Console.ReadLine(), out int id))
                        {
                            Console.WriteLine("Bad Format.");
                            break;
                        }

                        Console.WriteLine(server.GetClientReportFromId(id));
                        break;


                    case "4":
                        Console.WriteLine("Type the id of the client you want to kick out:");
                        if (!int.TryParse(Console.ReadLine(), out id))
                        {
                            Console.WriteLine("Bad Format.");
                            break;
                        }

                        Console.WriteLine($"Trying to disconnect client of id {id}...");

                        if (server.DisconnectClientById(id)) Console.WriteLine("Client disconnected successfully.");
                        else Console.WriteLine("Failed to disconnect client.");

                        break;


                    case "5":
                        server.DisconnectAllClients();
                        break;


                    case "10":
                        server.StopServer();
                        break;


                    default:
                        Console.WriteLine("Invalid input.");
                        break;
                }
            }

            //server.StopServer();
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

        public static void PrintServerMenu(bool isServerOn)
        {
            string msgOut = "";
            // ----- Menu Title -----
            //msgOut += "\t-\t-\tSERVER MENU - Status: ";
            Console.Write("\t-\t-\tSERVER MENU - Status: ");
            
            if (isServerOn)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("ON");
            } else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("OFF");
            }
            Console.ResetColor();

            Console.WriteLine("\t-\t-");
                          
            // ----- Menu Items -----

            msgOut += "\n1. Start Server" +
                      "\n2. Show clients" +
                      "\n3. Get Client Report" +
                      "\n4. Kick client" +
                      "\n5. Kick all clients (WIP)" +
                      "\n10. Close server and quit. (WIP)";
            Console.WriteLine(msgOut);
        }

    }
}
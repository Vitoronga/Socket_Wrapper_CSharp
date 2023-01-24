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

            while (input != "10")
            {
                PrintServerMenu(server.IsServerOnline());

                input = Console.ReadLine();
                Console.WriteLine("-\t-\t-\tHandling Input...\t-\t-\t");

                switch (input)
                {
                    case "1":
                        server.Start(20100);
                        break;


                    case "2":
                        Console.WriteLine("\nType your message: ");
                        string msgOut = Console.ReadLine();
                        server.SendDataAsServer(Encoding.ASCII.GetBytes($"[SERVER]: {msgOut}"));
                        break;


                    case "3":
                        Console.WriteLine($"Clients: {server.GetClientsStatus()}");
                        break;


                    case "4":
                        Console.WriteLine("Type the id of the client you want to inspect:");
                        if (!int.TryParse(Console.ReadLine(), out int id))
                        {
                            Console.WriteLine("Bad Format.");
                            break;
                        }

                        Console.WriteLine(server.GetClientReportFromId(id));
                        break;


                    case "5":
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


                    case "6":
                        server.DisconnectAllClients();
                        Console.WriteLine("Disconnection process completed.");
                        break;


                    case "10":
                        server.StopServer();
                        break;


                    default:
                        Console.WriteLine("Invalid input.\n");
                        break;
                }
            }

            // Out of socket connection context
        }

        private static void ClientHandler()
        {
            string input = "";

            SocketClient client = new SocketClient();
            if (!client.Connect("127.0.0.1", 20100))
            {
                Console.WriteLine("Failed to connect.");
                return;
            }

            Console.WriteLine("Configuring client. Type S for sender or R for receiver");
            input = Console.ReadLine();

            if (input.ToUpper().Equals("S"))
            {
                input = "Test";
                Console.WriteLine("Sender mode: Type your message after the '>>>', or !quit to disconnect.");
                do
                {
                    Console.Write(">>> ");
                    input = Console.ReadLine();

                    if (input == "!quit") break;
                    
                    bool success = client.SendData(new SocketMessage(input));
                    //Console.WriteLine("[DEBUG] Expected output: " + (string)SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(input))[0]);

                    if (!success) Console.WriteLine("<<< Failed to send data >>>");

                } while (client.IsConnected());
            }
            else
            {
                Console.WriteLine("Receiver mode: All incoming messages will be displayed as they come.");
                while (client.IsConnected())
                {
                    if (!client.ReceiveData(out byte[] bytes)) continue;
                    SocketMessage message = SocketMessage.UnformatByteArrayToClass(bytes);
                    Console.WriteLine(SocketMessageProtocol.GetUnformattedBytes(message.Data)[0]);
                }
            }

            client.CloseConnection();
        }

        public static void PrintServerMenu(bool isServerOn)
        {
            string msgOut = "";

            // ----- Menu Title -----
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
                      "\n2. Send Message to Clients" +
                      "\n3. Show clients" +
                      "\n4. Get Client Report" +
                      "\n5. Kick client" +
                      "\n6. Kick all clients" +
                      "\n10. Close server and quit.";
            Console.WriteLine(msgOut);
        }

    }
}
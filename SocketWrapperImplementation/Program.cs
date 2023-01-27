using SocketWrapperLibrary;
using System.Diagnostics;
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

                //DoDebugSession(client);

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

        private static void DoDebugSession(SocketClient client)
        {
            Console.WriteLine("[DEBUG] Doing stress test:");

            for (int i = 0; i < 3; i++)
            {
                string bigText = Return500bytesText(1);
                client.SendData(new SocketMessage(bigText));
            }

            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Sending 30kb of string: ");

            stopwatch.Start();
            string bigText2 = Return500bytesText(60);
            client.SendData(new SocketMessage(bigText2));
            stopwatch.Stop();
            Console.WriteLine($"Time elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            Console.WriteLine("Testing all types of data:");
            
            // Byte
            byte v_byte = 102;
            stopwatch.Start();
            object o_byte = (byte)SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_byte))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_byte} \n[OUTPUT] {o_byte} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Short
            short v_short = 30088;
            stopwatch.Start();
            object o_short = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_short))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_short} \n[OUTPUT] {o_short} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Int
            int v_int = 12003004;
            stopwatch.Start();
            object o_int = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_int))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_int} \n[OUTPUT] {o_int} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Long
            long v_long = 99;
            stopwatch.Start();
            object o_long = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_long))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_long} \n[OUTPUT] {o_long} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Float
            float v_float = 532.02384f;
            stopwatch.Start();
            object o_float = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_float))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_float} \n[OUTPUT] {o_float} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Double
            double v_double = 0.928382323123;
            stopwatch.Start();
            object o_double = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_double))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_double} \n[OUTPUT] {o_double} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Char
            char v_char = 'R';
            stopwatch.Start();
            object o_char = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_char))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_char} \n[OUTPUT] {o_char} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // String
            string v_string = "Hello world!";
            stopwatch.Start();
            object o_string = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetFormattedValue(v_string))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_string} \n[OUTPUT] {o_string} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Bool
            bool v_bool = true;
            stopwatch.Start();
            object o_bool = SocketMessageProtocol.GetUnformattedBytes(new byte[] { SocketMessageProtocol.GetFormattedBoolValues(v_bool) })[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_bool} \n[OUTPUT] {o_bool} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();

            // Bools
            bool[] v_bools = new bool[] { true, false, false };
            stopwatch.Start();
            object o_bools = SocketMessageProtocol.GetUnformattedBytes(SocketMessageProtocol.GetBoolArrayAsByteArray(v_bools))[0];
            stopwatch.Stop();
            Console.WriteLine($"[INPUT] {v_bools} \n[OUTPUT] {o_bools} \nTime Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
            stopwatch.Reset();
        }

        private static string Return500bytesText(int multiplier = 1)
        {
            const string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque mollis gravida velit a feugiat. Vestibulum accumsan, lacus a pretium elementum, turpis lectus rhoncus tortor, ut faucibus lacus neque ac est. Vestibulum ac auctor dolor. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. In quis eros quis purus lobortis finibus. Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce eget ornare ipsum, at imperdiet magna. Suspendisse efficitur.";
            StringBuilder builder = new StringBuilder(lorem);

            for (int i = 0; i < multiplier; i++)
            {
                builder.AppendLine(lorem);
            }

            return builder.ToString();
        }
    }
}
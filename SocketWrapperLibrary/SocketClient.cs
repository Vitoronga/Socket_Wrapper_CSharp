using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace SocketWrapperLibrary
{
    public class SocketClient
    {
        protected Socket? socket;

        public bool Connect(string ip, int port)
        {
            if (socket != null) return false;

            try
            {
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ip, port);

            } catch (Exception e)
            {
                Console.WriteLine("ERROR (Connect): " + e.Message);
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }

                return false;
            }

            return true;
        }

        internal bool SendData(byte[] bytes) // Main Method - for normal transmition and directly accessible only to server relays
        {
            if (socket == null) return false;

            // Adding message length inside itself
            byte[] finalBytes = new byte[bytes.Length + 4];
            BitConverter.GetBytes(bytes.Length).CopyTo(finalBytes, 0);
            Array.Copy(bytes, 0, finalBytes, 4, bytes.Length);

            try
            {
                int totalSent = socket.Send(finalBytes);

                while (totalSent < finalBytes.Length)
                {
                    byte[] currentArray = new byte[8192]; // 8192 is the current Socket packet/buffer length
                    Array.Copy(finalBytes, totalSent, currentArray, 0, currentArray.Length);
                    totalSent += socket.Send(currentArray);
                }
            } catch (Exception e)
            {
                Console.WriteLine("ERROR (SendData): " + e.Message);
                return false;
            }

            return true;
        }

        public bool SendData(ISocketMessage message)
        {
            if (socket == null) return false;

            return SendData(message.FormatDataAsByteArray());
        }

        public bool ReceiveData(out byte[] formattedBytes)
        {
            formattedBytes = new byte[0];

            if (socket == null) return false;

            try
            {
                byte[] receivedBytes = new byte[socket.ReceiveBufferSize];
                int totalBytesReceived = socket.Receive(receivedBytes) - 4;
                int totalBytesToBeReceived = BitConverter.ToInt32(receivedBytes, 0);

                formattedBytes = new byte[totalBytesToBeReceived];

                if (totalBytesReceived < totalBytesToBeReceived)
                {
                    Array.Copy(receivedBytes, 4, formattedBytes, 0, totalBytesReceived);

                    while (totalBytesReceived < totalBytesToBeReceived)
                    {
                        int currentReceived = socket.Receive(receivedBytes);
                        Array.Copy(receivedBytes, 0, formattedBytes, totalBytesReceived, currentReceived);
                        totalBytesReceived += currentReceived;
                    }
                } else
                {
                    Array.Copy(receivedBytes, 4, formattedBytes, 0, totalBytesToBeReceived);
                }

            } catch (Exception e)
            {
                Console.WriteLine("ERROR (ReceiveData): " + e.Message);
                return false;
            }

            return true;
        }

        public bool IsConnected() => socket != null && socket.Connected;

        public void CloseConnection()
        {
            if (socket == null) return;

            socket.Close();
        }
    }
}
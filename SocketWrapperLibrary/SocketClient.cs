using System.Net;
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

        public bool SendData(byte[] bytes)
        {
            if (socket == null) return false;

            try
            {
                socket.Send(bytes);
            } catch (Exception e)
            {
                Console.WriteLine("ERROR (SendData): " + e.Message);
                return false;
            }

            return true;
        }

        public bool ReceiveData(out byte[] formattedBytes) // Maybe return bool and pass the received bytes as out parameter?
        {
            formattedBytes = new byte[0];

            if (socket == null) return false;

            try
            {
                byte[] receivedBytes = new byte[socket.ReceiveBufferSize];
                int byteAmount = socket.Receive(receivedBytes);
                formattedBytes = new byte[byteAmount];

                for (int i = 0; i < formattedBytes.Length; i++)
                {
                    formattedBytes[i] = receivedBytes[i];
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
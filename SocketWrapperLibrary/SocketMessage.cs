using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketWrapperLibrary
{
    public class SocketMessage : ISocketMessage
    {
        private readonly byte[] data;
        public byte[] Data { get { return data; } }

        private readonly byte classId;
        public byte ClassId { get { return classId; } }

        //public readonly DateTime creationTime;
        //public DateTime CreationTime { get { return creationTime; } }

        public SocketMessage(byte[] data, byte classId) {
            this.data = data;
            this.classId = classId;
            
            //this.creationTime = DateTime.Now;
        }

        public SocketMessage(byte data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(short data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(int data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(long data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(float data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(double data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(char data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(string data, byte classId = 0) : this(SocketMessageProtocol.GetFormattedValue(data), classId) { }
        public SocketMessage(bool data, byte classId = 0) : this(new byte[] { SocketMessageProtocol.GetFormattedBoolValues(data) }, classId) { }

        public byte[] FormatDataAsByteArray()
        {
            byte[] formattedData = new byte[Data.Length + 1]; // ClassId slot + Whole class data

            formattedData[0] = ClassId;
            //SocketMessageProtocol.GetFormattedValue(CreationTime.Ticks).CopyTo(formattedData, 1);
            Data.CopyTo(formattedData, 1);

            return formattedData;
        }

        public static SocketMessage UnformatByteArrayToClass(byte[] data)
        {
            // Problem: How to properly recreate the class from the received data? Values are all readonly...

            // Get Headers:
            byte tag = data[0];

            // Handle Values:
            byte[] rawData = new byte[data.Length - 1];
            Array.Copy(data, 1, rawData, 0, data.Length - 1);
            //List<object> values = SocketMessageProtocol.GetUnformattedBytes(rawData); // This is what a dedicated class would normally do.
            SocketMessage sm = new SocketMessage(rawData, tag); // Re-setting data to be the raw one. User will need to call SocketMessageProtocol.GetUnformattedBytes(data) manually when using this class

            return sm;
        }
    }
}

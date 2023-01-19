using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SocketWrapperLibrary
{
    public class SocketMessageProtocol
    {
        public enum DataSignatures
        {
            Byte = 0,
            Short = 1,
            Int = 2,
            Float = 3,
            Char = 4,
            String = 5,     // Protocol: 1 byte for signature ; 2 bytes (short) for char count, 2 byte per char
            Bool = 15       // Protocol: First 4 bits ON (or 1) for signature ; pass 4 bool values
        }

        //
        // Formatting methods:
        //

        public static byte[] GetFormattedValue(byte value)
        {
            byte[] byteArray = new byte[2]; // byte size + data signature byte

            byteArray[0] = (byte)DataSignatures.Byte; // set first slot for data signature (so the signature comes first in every case)
            byteArray[1] = value;

            return byteArray;
        }

        public static byte[] GetFormattedValue(short value)
        {
            byte[] byteArray = new byte[3]; // short size + data signature byte

            byteArray[0] = (byte)DataSignatures.Short; // set first slot for data signature (so the signature comes first in every case)
            BitConverter.GetBytes(value).CopyTo(byteArray, 1);

            return byteArray;
        }

        public static byte[] GetFormattedValue(int value)
        {
            byte[] byteArray = new byte[5]; // int size + data signature byte

            byteArray[0] = (byte)DataSignatures.Int; // set first slot for data signature (so the signature comes first in every case)
            BitConverter.GetBytes(value).CopyTo(byteArray, 1);

            return byteArray;
        }

        public static byte[] GetFormattedValue(float value)
        {
            byte[] byteArray = new byte[5]; // float size + data signature byte

            byteArray[0] = (byte)DataSignatures.Float; // set first slot for data signature (so the signature comes first in every case)
            BitConverter.GetBytes(value).CopyTo(byteArray, 1);

            return byteArray;
        }
        
        public static byte[] GetFormattedValue(char value)
        {
            byte[] byteArray = new byte[3]; // char size + data signature byte

            byteArray[0] = (byte)DataSignatures.Char; // set first slot for data signature (so the signature comes first in every case)
            BitConverter.GetBytes(value).CopyTo(byteArray, 1);

            return byteArray;
        }

        public static byte[] GetFormattedValue(string value)
        {
            byte[] byteArray = new byte[value.Length * 2 + 3]; // string size + (data signature byte + char count (short size))

            byteArray[0] = (byte)DataSignatures.String; // set first slot for data signature (so the signature comes first in every case)            
            BitConverter.GetBytes(value.Length).CopyTo(byteArray, 1); // set second slot for char count
            ByteHelper.ConvertToByteArray(value).CopyTo(byteArray, 3); // fourth slot start the actual value

            return byteArray;
        }

        public static byte GetFormattedBoolValues(bool value1, bool value2 = false, bool value3 = false, bool value4 = false)
        {
            byte byteValue = (byte)SocketMessageProtocol.DataSignatures.Bool; // Adding data signature (for later recognition)

            byteValue += (byte)((((value1 ? 1 : 0) << 3) + ((value2 ? 1 : 0) << 2) + ((value3 ? 1 : 0) << 1) + (value4 ? 1 : 0)) << 4);

            return byteValue;
        }

        public static byte[] GetBoolArrayAsByteArray(bool[] boolArray)
        {
            byte[] byteArray;

            if (boolArray.Length > 4)
            {
                int length = boolArray.Length;
                int subArrayAmount = length % 4 + (length % 4 == 0 ? 0 : 1);
                byteArray = new byte[subArrayAmount];

                for (int i = 0; i < subArrayAmount; i++)
                {
                    bool[] currentArray = new bool[4];
                    boolArray.CopyTo(currentArray, 4 * i);
                    GetBoolArrayAsByteArray(currentArray).CopyTo(byteArray, 4 * i);
                }
            } 
            else
            {
                byte byteValue = 0;

                switch (boolArray.Length)
                {
                    case 1:
                        byteValue = GetFormattedBoolValues(boolArray[0]);
                        break;

                    case 2:
                        byteValue = GetFormattedBoolValues(boolArray[0], boolArray[1]);
                        break;

                    case 3:
                        byteValue = GetFormattedBoolValues(boolArray[0], boolArray[1], boolArray[2]);
                        break;

                    case 4:
                        byteValue = GetFormattedBoolValues(boolArray[0], boolArray[1], boolArray[2], boolArray[3]);
                        break;
                }

                byteArray = new byte[] { byteValue };
            }

            return byteArray;
        }
    
        //
        // TODO: Unformatting methods:
        //

        // Methods goes here
    }
}

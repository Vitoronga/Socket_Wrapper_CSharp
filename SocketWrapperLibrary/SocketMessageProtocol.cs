using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketWrapperLibrary
{
    public class SocketMessageProtocol
    {
        public enum DataSignatures
        {
            Byte = 0,
            Short = 1,
            Int = 2,
            Long = 3,
            Float = 4,
            Double = 5,
            Char = 6,
            String = 7,     // Protocol: 1 byte for signature ; 2 bytes (short) for char count, 2 byte per char
            // Keep bool as the biggest value!
            Bool = 15       // Protocol: First 4 bits ON (or 1) for signature ; pass 3 bool values + a delimiter bit
        }

        internal static readonly Type STRING_LENGTH_LIMIT = typeof(short);
        internal static readonly int BOOL_ARRAY_LENGTH_LIMIT = 3;

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

        public static byte[] GetFormattedValue(long value)
        {
            byte[] byteArray = new byte[9]; // long size + data signature byte

            byteArray[0] = (byte)DataSignatures.Long; // set first slot for data signature (so the signature comes first in every case)
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

        public static byte[] GetFormattedValue(double value)
        {
            byte[] byteArray = new byte[9]; // double size + data signature byte

            byteArray[0] = (byte)DataSignatures.Double; // set first slot for data signature (so the signature comes first in every case)
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
            if (value.Length > short.MaxValue) throw new Exception("ERROR: String size is too big (>32767)");
            short length = (short)value.Length;

            byte[] byteArray = new byte[length * 2 + 3]; // string size + (data signature byte + char count (short size))

            byteArray[0] = (byte)DataSignatures.String; // set first slot for data signature (so the signature comes first in every case)            
            BitConverter.GetBytes(length).CopyTo(byteArray, 1); // set second slot for char count
            ByteHelper.ConvertToByteArray(value).CopyTo(byteArray, 3); // fourth slot start the actual value

            return byteArray;
        }

        public static byte GetFormattedBoolValues(bool value1)
        {
            byte byteValue = (byte)SocketMessageProtocol.DataSignatures.Bool; // Adding data signature (for later recognition)

            byteValue += (byte)(((1 << 1) + ((value1 ? 1 : 0))) << 4);

            return byteValue;
        }

        public static byte GetFormattedBoolValues(bool value1, bool value2)
        {
            byte byteValue = (byte)SocketMessageProtocol.DataSignatures.Bool; // Adding data signature (for later recognition)

            byteValue += (byte)(((1 << 2) + ((value1 ? 1 : 0) << 1) + ((value2 ? 1 : 0))) << 4);

            return byteValue;
        }

        public static byte GetFormattedBoolValues(bool value1, bool value2, bool value3)
        {
            byte byteValue = (byte)SocketMessageProtocol.DataSignatures.Bool; // Adding data signature (for later recognition)

            byteValue += (byte)(((1 << 3) + ((value1 ? 1 : 0) << 2) + ((value2 ? 1 : 0) << 1) + ((value3 ? 1 : 0))) << 4);

            return byteValue;
        }

        public static byte[] GetBoolArrayAsByteArray(bool[] boolArray)
        {
            byte[] byteArray;

            if (boolArray.Length > BOOL_ARRAY_LENGTH_LIMIT)
            {
                int length = boolArray.Length;
                int subArrayAmount = length % BOOL_ARRAY_LENGTH_LIMIT + (length % BOOL_ARRAY_LENGTH_LIMIT == 0 ? 0 : 1);
                byteArray = new byte[subArrayAmount];

                for (int i = 0; i < subArrayAmount; i++)
                {
                    bool[] currentArray = new bool[BOOL_ARRAY_LENGTH_LIMIT];
                    Array.Copy(boolArray, BOOL_ARRAY_LENGTH_LIMIT * i, currentArray, 0, currentArray.Length);
                    GetBoolArrayAsByteArray(currentArray).CopyTo(byteArray, BOOL_ARRAY_LENGTH_LIMIT * i);
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
                }

                byteArray = new byte[] { byteValue };
            }

            return byteArray;
        }
    
        //
        // Unformatting methods:
        //

        public static List<object> GetUnformattedBytes(byte[] data)
        {
            List<object> values = new List<object>();

            for (int i = 0; i < data.Length; ) // Keep no increment
            {
                DataSignatures sig = (data[i] >= (byte)DataSignatures.Bool ? DataSignatures.Bool : (DataSignatures)data[i]);
                i++;

                switch (sig)
                {
                    case (DataSignatures.Byte):
                        values.Add(data[i++]);
                        break;

                    case (DataSignatures.Short):
                        values.Add(BitConverter.ToInt16(data, i));
                        i += 2;
                        break;

                    case (DataSignatures.Int):
                        values.Add(BitConverter.ToInt32(data, i));
                        i += 4;
                        break;

                    case (DataSignatures.Long):
                        values.Add(BitConverter.ToInt64(data, i));
                        i += 8;
                        break;

                    case (DataSignatures.Float):
                        values.Add(BitConverter.ToSingle(data, i));
                        i += 4;
                        break;

                    case (DataSignatures.Double):
                        values.Add(BitConverter.ToDouble(data, i));
                        i += 8;
                        break;

                    case (DataSignatures.Char):
                        values.Add(BitConverter.ToChar(data, i));
                        i += 2;
                        break;

                    case (DataSignatures.String):
                        short charAmount = BitConverter.ToInt16(data, i);
                        i += 2;
                        StringBuilder builder = new StringBuilder();

                        for (int j = 0; j < charAmount; j++)
                        {
                            builder.Append(BitConverter.ToChar(data, i));
                            i += 2;
                        }

                        values.Add(builder.ToString());

                        break;

                    case (DataSignatures.Bool):
                        byte bools = (byte)(data[i] >> 4);
                        BitArray bitArray = new BitArray(new byte[] { bools });

                        bool foundDelimiter = false;
                        for (int j = BOOL_ARRAY_LENGTH_LIMIT; j >= 0; j--)
                        {
                            if (!foundDelimiter) foundDelimiter = bitArray[j]; // As bitArray stores a bunch of boolean values, and I want to find the first 1 bit, just store the value until it finds it.
                            else values.Add(bitArray[j]);
                        }

                        i++;

                        break;

                    default:
                        Console.WriteLine($"Could not identify the type of following byte: { data[i] }");
                        break;
                }
            }

            return values;
        }
    }
}

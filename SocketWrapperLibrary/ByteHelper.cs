using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketWrapperLibrary
{
    public class ByteHelper
    {
        public static byte[] ConvertToByteArray(string text)
        {
            byte[] byteArray;
            byteArray = new byte[text.Length * 2];

            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                byte[] letter = BitConverter.GetBytes(text[i]);
                //Console.WriteLine("l: " + text[i]);
                foreach (byte b in letter)
                {
                    //Console.WriteLine(b);
                    byteArray[index++] = b;
                }
            }

            return byteArray;
        }

        public static string ConvertToString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();

            Console.WriteLine(bytes.Length);

            for (int i = 0; i < bytes.Length; i += 2)
            {
                builder.Append(BitConverter.ToChar(bytes, i));
            }

            return builder.ToString();
        }
    }
}

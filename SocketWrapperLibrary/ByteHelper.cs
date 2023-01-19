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
            byteArray = new byte[text.Length * 2]; // Each char inside text seems to be converted to a hexadecimal rep of its bytes + a blank byte (ex. 78-00) (imprecise)

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
            string text = "";

            Console.WriteLine(bytes.Length);

            for (int i = 0; i < bytes.Length; i += 2)
            {
                text += BitConverter.ToChar(bytes, i);
            }

            return text;
        }
    }
}

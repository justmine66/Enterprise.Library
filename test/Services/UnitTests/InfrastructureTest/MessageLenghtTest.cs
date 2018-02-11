using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureTest
{
    public class MessageLenghtTest
    {
        public static void Test()
        {
            byte[] lengthBytes = FrameLength(1900);
            UnFrameLength(lengthBytes);
        }
        public static byte[] FrameLength(int length)
        {
            Console.WriteLine("正常包长度({0}).", length);
            var lengthBytes = new byte[4] { (byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24) };
            return lengthBytes;
        }

        public static void UnFrameLength(byte[] lengthBytes)
        {
            int numHeaderBytes = 0;
            int packageLength = 0;
            int headLength = 4;
            for (int i = 0; i < lengthBytes.Length; i++)
            {
                if (numHeaderBytes < headLength)
                {
                    int unit = lengthBytes[i] << (numHeaderBytes * 8);
                    packageLength |= unit;
                    ++numHeaderBytes;

                    if (numHeaderBytes == headLength)
                    {
                        if (packageLength <= 0)
                        {
                            Console.WriteLine(string.Format("Package length ({0}) is out of bounds.", packageLength));
                        }

                        Console.WriteLine("正确解析包长度({0}).", packageLength);
                        Console.Read();
                    }
                }
            }
        }
    }
}

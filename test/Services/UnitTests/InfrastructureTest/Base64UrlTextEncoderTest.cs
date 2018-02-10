using Enterprise.Library.Common.Encoders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureTest
{
    public class Base64UrlTextEncoderTest
    {
        public static void Test()
        {
            string data = "你好吗的=";
            string base64UrlEncodedText = Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(data));
            byte[] dataDecoded = Base64UrlTextEncoder.Decode(base64UrlEncodedText);

            Console.WriteLine("要进行base64编码的数据: {0}", data);
            Console.WriteLine("通过base64解码返回的数据: {0}", Encoding.UTF8.GetString(dataDecoded));

            Console.Read();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace InfrastructureTest
{
    public class SpanTest
    {
        public static void Test()
        {
            
        }

        public static void RefReturnTest()
        {
            var listOfStructs = new List<MutableStruct>() { new MutableStruct() };
            listOfStructs[0].Value = 42;
        }
    }

    public class MutableStruct { public int Value; }
}

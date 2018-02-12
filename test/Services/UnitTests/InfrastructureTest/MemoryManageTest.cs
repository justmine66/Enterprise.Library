using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureTest
{
    public class MemoryManageTest
    {
        public static void Test()
        {
            List<byte[]> buffer1 = new List<byte[]>();
            List<byte[]> buffer2 = new List<byte[]>();
            List<byte[]> buffer3 = new List<byte[]>();

            //            
            //    allocate             
            //            
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("1. Allocate 64mb block(s) as more as possible...");
            try
            {
                while (true)
                {
                    buffer1.Add(new byte[64 * 1024 * 1024]);
                    Console.Write("#");
                    buffer2.Add(new byte[64 * 1024 * 1024]);
                    Console.Write("#");
                }
            }
            catch (OutOfMemoryException)
            {
            }
            Console.WriteLine();
            Console.WriteLine("   Total {0} blocks were allocated ( {1} MB).", (buffer1.Count + buffer2.Count), (buffer1.Count + buffer2.Count) * 64);

            //        
            //    free  
            //        
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("2. Free Blocks...");
            buffer2.Clear();
            Console.WriteLine("   Total: {0} blocks ({1} MB)", buffer1.Count, buffer1.Count * 64);

            //        
            //  GC  
            //            
            GC.Collect(GC.MaxGeneration);

            //           
            //    allocate  
            //          
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("3. Allocate 72mb block(s) as more as possible...");
            try
            {
                while (true)
                {
                    buffer3.Add(new byte[72 * 1024 * 1024]);
                    Console.Write("#");
                }
            }
            catch (OutOfMemoryException)
            {
            }
            Console.WriteLine();
            Console.WriteLine("   Total: 64mb x {0}, 72mb x {1} blocks allocated( {2} MB).\n", buffer1.Count, buffer3.Count, buffer1.Count * 64 + buffer3.Count * 72);
            Console.ReadLine();
        }
    }
}

namespace BufferPoolTest
{
    using Enterprise.Library.Common.Socketing.Buffering;

    public class BufferPoolTest
    {
        public static void Get_item_of_buffer_pool()
        {
            var bufferPool = new BufferPool(1024 * 64, 50);
            byte[] item = bufferPool.Get();
        }

        public static void Expanding_buffer_pool()
        {
            var bufferPool = new BufferPool(1024 * 64, 1);
            byte[] item1 = bufferPool.Get();
            byte[] item2 = bufferPool.Get();
        }

        public static void Return_item_to_buffer_pool()
        {
            var bufferPool = new BufferPool(1024 * 64, 1);
            byte[] item = bufferPool.Get();
            bufferPool.Return(item);
        }
    }
}

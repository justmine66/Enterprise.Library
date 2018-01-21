using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    struct PoolItemState
    {
        public byte Generation { get; set; }
    }

    /// <summary>
    /// Intelligent pool base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IntelliPoolBase<T> : IPool<T>
    {
        private ConcurrentStack<T> _store;
        private IPoolItemCreator<T> _itemCreator;
        private byte _currentGeneration = 0;
        private int _nextExpandThreshold;
        private int _inExpanding = 0;
        private int _totalCount;
        private int _availableCount;
        private Action<T> _itemCleaner;
        private Action<T> _itemPreGet;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntelliPoolBase{T}"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="itemCreator">The item creator.</param>
        /// <param name="itemCleaner">The item cleaner.</param>
        /// <param name="itemPreGet">The item pre get.</param>
        public IntelliPoolBase(int initialCount, IPoolItemCreator<T> itemCreator, Action<T> itemCleaner = null, Action<T> itemPreGet = null)
        {
            this._itemCreator = itemCreator;
            this._itemCleaner = itemCleaner;
            this._itemPreGet = itemPreGet;

            var list = new List<T>(initialCount);

            foreach (var item in itemCreator.Create(initialCount))
            {
                this.RegisterNewItem(item);
                list.Add(item);
            }

            _store = new ConcurrentStack<T>(list);

            this._totalCount = initialCount;
            this._availableCount = _totalCount;
            this.UpdateNextExpandThreshold();
        }

        /// <summary>
        /// Registers the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected abstract void RegisterNewItem(T item);

        /// <summary>
        /// Gets the current generation.
        /// </summary>
        /// <value>
        /// The current generation.
        /// </value>
        protected byte CurrentGeneration
        {
            get { return _currentGeneration; }
        }

        /// <summary>
        /// Gets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount
        {
            get { return _totalCount; }
        }

        /// <summary>
        /// Gets the available count, the items count which are available to be used.
        /// </summary>
        /// <value>
        /// The available count.
        /// </value>
        public int AvailableCount
        {
            get { return _availableCount; }
        }

        /// <summary>
        /// Gets an item from the pool.
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            if (this._store.TryPop(out T item))
            {
                Interlocked.Decrement(ref this._availableCount);
                
                //require expand
                if (this.AvailableCount <= this._nextExpandThreshold && this._inExpanding == 0)
                {
                    Task.Run(() => { this.TryExpand(); });
                }

                var itemPreGet = this._itemPreGet;
                if (itemPreGet != null)
                {
                    itemPreGet(item);
                }

                return item;
            }

            //In expanding
            if (this._inExpanding == 1)
            {
                var spinner = new SpinWait();

                while (true)
                {
                    spinner.SpinOnce();

                    if (_store.TryPop(out item))
                    {
                        Interlocked.Decrement(ref _availableCount);

                        var itemPreGet = _itemPreGet;

                        if (itemPreGet != null)
                            itemPreGet(item);

                        return item;
                    }

                    if (_inExpanding != 1)
                        return Get();
                }
            }
            else
            {
                this.TryExpand();
                this.Get();
            }
        }

        public void Return(T item)
        {
            throw new NotImplementedException();
        }

        public bool Shrink()
        {
            throw new NotImplementedException();
        }

        void UpdateNextExpandThreshold()
        {
            _nextExpandThreshold = _totalCount / 5; //if only 20% buffer left, we can expand the buffer count
        }
        bool TryExpand()
        {
            if (Interlocked.CompareExchange(ref this._inExpanding, 1, 0) != 0)
            {
                return false;
            }

            this.Expand();
            this._inExpanding = 0;
            return true;
        }
        void Expand()
        {
            int totalCount = this._totalCount;

            foreach (T item in this._itemCreator.Create(totalCount))
            {
                this._store.Push(item);
                Interlocked.Increment(ref this._availableCount);
                this.RegisterNewItem(item);
            }

            this._currentGeneration++;
            this._totalCount += totalCount;
            this.UpdateNextExpandThreshold();
        }
    }
}

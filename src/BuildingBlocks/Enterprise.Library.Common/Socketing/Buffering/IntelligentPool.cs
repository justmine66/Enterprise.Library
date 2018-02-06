using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Library.Common.Socketing.Buffering
{
    /// <summary>
    /// state of pool item
    /// </summary>
    public struct PoolItemState
    {
        /// <summary>
        /// the current generation to pool.
        /// </summary>
        public byte Generation { get; set; }
    }

    /// <summary>
    /// intelligent pool supporting auto-expand
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IntelligentPool<T> : IntelligentPoolBase<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntelliPool{T}"/> class.
        /// </summary>
        /// <param name="initialCount">The initial count.</param>
        /// <param name="itemCreator">The item creator.</param>
        /// <param name="itemCleaner">The item cleaner.</param>
        /// <param name="itemPreGet">The item pre get.</param>
        public IntelligentPool(int initialCount,
            IPoolItemCreator<T> itemCreator,
            Action<T> itemCleaner = null,
            Action<T> itemPreGet = null)
            : base(initialCount, itemCreator, itemCleaner, itemPreGet)
        {

        }

        private ConcurrentDictionary<T, PoolItemState> _bufferDict = new ConcurrentDictionary<T, PoolItemState>();
        private ConcurrentDictionary<T, T> _removedItemDict;

        /// <summary>
        /// Determines whether the specified item can be returned.
        /// </summary>
        /// <param name="item">The item to be returned.</param>
        /// <returns>
        ///   <c>true</c> if the specified item can be returned; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanReturn(T item)
        {
            return this._bufferDict.ContainsKey(item);
        }

        /// <summary>
        /// Registers the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected override void RegisterNewItem(T item)
        {
            var state = new PoolItemState();
            state.Generation = this.CurrentGeneration;
            this._bufferDict.TryAdd(item, state);
        }

        /// <summary>
        /// Tries to remove the specific item
        /// </summary>
        /// <param name="item">The specific item to be removed.</param>
        /// <returns></returns>
        protected override bool TryRemove(T item)
        {
            if (_removedItemDict == null || _removedItemDict.Count == 0)
            {
                return false;
            }

            return _removedItemDict.TryRemove(item, out T removedItem);
        }

        /// <summary>
        /// Shrinks this instance.
        /// </summary>
        /// <returns></returns>
        public override bool Shrink()
        {
            var generation = CurrentGeneration;
            if (!base.Shrink())
            {
                return false;
            }

            var toBeRemoved = new List<T>(TotalCount / 2);

            foreach (var item in _bufferDict)
            {
                if (item.Value.Generation == generation)
                {
                    toBeRemoved.Add(item.Key);
                }
            }

            if (_removedItemDict == null)
                _removedItemDict = new ConcurrentDictionary<T, T>();

            foreach (var item in toBeRemoved)
            {
                if (_bufferDict.TryRemove(item, out PoolItemState state))
                {
                    _removedItemDict.TryAdd(item, item);
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Intelligent pool base class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class IntelligentPoolBase<T> : IPool<T>
    {
        #region [ Private fields and constructors ]

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
        public IntelligentPoolBase(
            int initialCount,
            IPoolItemCreator<T> itemCreator,
            Action<T> itemCleaner = null,
            Action<T> itemPreGet = null)
        {
            if (initialCount <= 0)
            {
                throw new InvalidOperationException("initial Count of pool can not is zero.");
            }

            if (itemCreator == null)
            {
                throw new ArgumentNullException("itemCreator is not null.");
            }

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

        #endregion

        #region [ public properties ]

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

        #endregion

        #region [ public methods ]

        /// <summary>
        /// Registers the new item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected abstract void RegisterNewItem(T item);

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

            //In expanding，lazy get...
            if (this._inExpanding == 1)
            {
                var spinner = new SpinWait();

                while (true)
                {
                    spinner.SpinOnce();//short stop

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
                return this.Get();
            }
        }

        /// <summary>
        /// Determines whether the specified item can be returned.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the specified item can be returned; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanReturn(T item);

        /// <summary>
        /// Tries to remove the specific item
        /// </summary>
        /// <param name="item">The specific item to be removed.</param>
        /// <returns></returns>
        protected abstract bool TryRemove(T item);

        /// <summary>
        /// Returns the specified item to the pool.
        /// </summary>
        /// <param name="item">the item.</param>
        public void Return(T item)
        {
            var itemCleanner = this._itemCleaner;
            if (itemCleanner != null)
            {
                itemCleanner(item);
            }

            if (this.CanReturn(item))
            {
                this._store.Push(item);
                Interlocked.Increment(ref this._availableCount);
                return;
            }

            if (this.TryRemove(item))
            {
                Interlocked.Decrement(ref this._totalCount);
            }
        }

        /// <summary>
        /// Shrinks this pool.
        /// </summary>
        /// <returns></returns>
        public virtual bool Shrink()
        {
            var generation = this._currentGeneration;
            if (generation == 0)
                return false;

            var shrinThreshold = _totalCount * 3 / 4;

            if (_availableCount <= shrinThreshold)
                return false;

            _currentGeneration = (byte)(generation - 1);
            return true;
        }

        #endregion

        #region [ internal methods ]

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

        #endregion
    }
}

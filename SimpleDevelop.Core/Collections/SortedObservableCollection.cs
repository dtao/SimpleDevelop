using System.Collections.Generic;
using System.Collections.Specialized;

namespace SimpleDevelop.Collections
{
    class SortedObservableCollection<T> : ListBase<T>, INotifyCollectionChanged
    {
        private readonly List<T> _list = new List<T>();
        private readonly IComparer<T> _comparer;
        
        public SortedObservableCollection(IComparer<T> comparer = null)
        {
            _comparer = comparer ?? Comparer<T>.Default;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public override T this[int index]
        {
            get { return _list[index]; }
            set
            {
                _list[index] = value;
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
            }
        }

        public override int Count
        {
            get { return _list.Count; }
        }

        public override void Add(T item)
        {
            int index = _list.BinarySearch(item, _comparer);
            if (index < 0)
            {
                index = ~index;
            }

            _list.Insert(index, item);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        protected void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                switch (action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        handler(this, new NotifyCollectionChangedEventArgs(action));
                        break;
                    default:
                        handler(this, new NotifyCollectionChangedEventArgs(action, item, index));
                        break;
                }
            }
        }
    }
}

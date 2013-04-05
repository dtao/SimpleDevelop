using System;
using System.Collections.Generic;

using SimpleDevelop.Core;

namespace SimpleDevelop.Collections
{
    abstract class ListBase<T> : IList<T>
    {
        public abstract T this[int index] { get; set; }

        public abstract int Count { get; }

        public abstract void Add(T item);

        public virtual int IndexOf(T item)
        {
            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

            for (int i = 0; i < Count; i++)
            {
                if (comparer.Equals(item, this[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        public virtual bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            array.ThrowIfNull("array");
            arrayIndex.ThrowIfOutside(0, Count, "arrayIndex");

            for (int i = 0; i < Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                ((IList<T>)this).RemoveAt(index);
                return true;
            }

            return false;
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

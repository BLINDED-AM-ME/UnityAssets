using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BLINDED_AM_ME
{
    /// <summary> Serves as a middle man to restrict what can go into another List </summary>
    /// <remarks> You can't cast a collection so here is a work around </remarks>
    /// T must inherit from U
    public class ListWrapper<T, U> : IList<T>, INotifyCollectionChanged where T : U
    {
        public delegate void ItemEventHandler(object sender, T item);
        public event ItemEventHandler ItemAdded;
        public event ItemEventHandler ItemRemoved;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        
        private IList<U> _items;

        public ListWrapper(IList<U> list)
        {
            _items = list;
        }

        public T this[int index]
        {
            get => (T)_items[index];
            set
            {
                var old = (T)_items[index];
                _items[index] = value;

                OnItemRemoved(old);
                OnItemAdded(value);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        protected virtual void OnItemAdded(T item)
        {
            ItemAdded?.Invoke(this, item);
        }
        protected virtual void OnItemRemoved(T item)
        {
            ItemRemoved?.Invoke(this, item);
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            var oldCount = Count;
            _items.Add(item);
            OnItemAdded(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }
        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            OnItemAdded(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public bool Remove(T item)
        {
            if (_items.Remove(item))
            {
                OnItemRemoved(item);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                return true;
            }

            return false;
        }
        public void RemoveAt(int index)
        {
            var old = this[index];

            _items.RemoveAt(index);

            OnItemRemoved(old);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
        }

        public void Clear()
        {
            var old = _items.ToList();
            _items.Clear();

            foreach (var x in old)
                OnItemRemoved((T)x);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, old));
        }

        public int IndexOf(T item) => _items.IndexOf(item);
        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex)
        {
            var data = new T[array.Length];
            for (int i = 0; i < Count; i++)
            {
                if (i + arrayIndex >= array.Length)
                    break;

                array[i + arrayIndex] = this[i];
            }
        }

        public IEnumerator<T> GetEnumerator() => new ListWrapperEnumerator(this);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // When you implement IEnumerable, you must also implement IEnumerator.
        private class ListWrapperEnumerator : IEnumerator<T>, IEnumerator
        {
            private ListWrapper<T, U> _list;

            // Enumerators are positioned before the first element
            // until the first MoveNext() call.
            private int position = -1;
            
            public T Current => (T)_list[position];
            object IEnumerator.Current => Current;

            public ListWrapperEnumerator(ListWrapper<T, U> list)
            {
                _list = list;
            }

            public void Reset()
            {
                position = -1;
            }

            public bool MoveNext()
            {
                position++;
                return position < _list.Count;
            }

            public void Dispose()
            {
                _list = null;
            }
        }
    }
}

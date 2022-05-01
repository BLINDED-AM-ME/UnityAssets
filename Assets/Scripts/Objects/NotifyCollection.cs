using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLINDED_AM_ME
{ 
    /// <summary> A List with events </summary>
    /// <remarks> Because ObservableCollection does not give removed items when Clear is Called </remarks>
    public class NotifyCollection<T> : IList<T>, INotifyCollectionChanged
    {
        private List<T> _items = new List<T>();

        public delegate void ItemEventHandler(object sender, T item);
        public event ItemEventHandler ItemAdded;
        public event ItemEventHandler ItemRemoved;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public T this[int index]
        {
            get => _items[index];
            set
            {
                var old = _items[index];
                if (!Equals(old, value))
                {
                    _items[index] = value;

                    OnItemRemoved(old);
                    OnItemAdded(value);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
                }
            }
        }

        public NotifyCollection() { }

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
        public void AddRange(IEnumerable<T> collection)
        {
            var oldCount = Count;
            _items.AddRange(collection);

            foreach (var item in collection)
                OnItemAdded(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection));
        }

        public void Insert(int index, T item)
        {
            _items.Insert(index, item);
            OnItemAdded(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _items.InsertRange(index, collection);

            foreach (var x in collection)
                OnItemAdded(x);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<T>(collection), index));
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
        public void RemoveRange(int index, int count)
        {
            var old = _items.GetRange(index, count);
            _items.RemoveRange(index, count);

            foreach (var x in old)
                OnItemRemoved(x);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old, index));
        }

        public void Clear()
        {
            var old = _items.ToList();
            _items.Clear();

            foreach (var x in old)
                OnItemRemoved(x);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, old));
        }

        public List<T> GetRange(int index, int count) => _items.GetRange(index, count);
        public int IndexOf(T item) => _items.IndexOf(item);
        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
    }
}

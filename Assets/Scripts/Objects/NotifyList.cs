using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BLINDED_AM_ME.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Objects
{
   
    public abstract class NotifyList { }

    /// <summary> List with events </summary>
    /// <remarks> Because ObservableCollection does not give the items when Clear() is called </remarks>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class NotifyList<T> : NotifyList, IList<T>, IList, INotifyCollectionChanged, ISerializationCallbackReceiver
    {
        public event ItemEventHandler<T> ItemAdded;
        public event ItemEventHandler<T> ItemRemoved;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private List<T> _items = new List<T>();

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

        object IList.this[int index]
        {
            get => ((IList)_items)[index];
            set
            {
                var old = _items[index];
                if (!Equals(old, value))
                {
                    _items[index] = (T)value;

                    OnItemRemoved(old);
                    OnItemAdded((T)value);
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old));
                }
            }
        }

        public NotifyList()
        {
            _items = new List<T>();
        }
        public NotifyList(int capacity)
        {
            _items = new List<T>(capacity);
        }
        public NotifyList(IEnumerable<T> collection)
        {
            _items = new List<T>(collection);
        }

        protected virtual void OnItemAdded(T item)
        {
            ItemAdded?.Invoke(this, new ItemEventArgs<T>(item));
        }
        protected virtual void OnItemRemoved(T item)
        {
            ItemRemoved?.Invoke(this, new ItemEventArgs<T>(item));
        }
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            CollectionChanged?.Invoke(this, args);
        }
        
        public int Count => _items.Count;
        public bool IsReadOnly => false;
        public bool IsFixedSize => ((IList)_items).IsFixedSize;
        public bool IsSynchronized => ((ICollection)_items).IsSynchronized;
        public object SyncRoot => ((ICollection)_items).SyncRoot;

        public int Add(object value)
        {
            var x = Count;
            Add((T)value);
            return x;
        }
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

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
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

        public void Remove(object value)
        {
            Remove((T)value);
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

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, old));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public List<T> GetRange(int index, int count) => _items.GetRange(index, count);

        public int IndexOf(object value) => ((IList)_items).IndexOf(value);
        public int IndexOf(T item) => _items.IndexOf(item);

        public bool Contains(object value) => ((IList)_items).Contains(value);
        public bool Contains(T item) => _items.Contains(item);

        public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

        // I need it to trigger the events
        [SerializeField]
        private List<T> _serializableItems = new List<T>();
        public void OnBeforeSerialize()
        {
            _serializableItems.Clear();
            foreach (T item in _items)
                _serializableItems.Add(item);
        }
        public void OnAfterDeserialize()
        {
            // Remove
            var deadItems = this.Where(item => !_serializableItems.Contains(item)).ToList();
            foreach (var item in deadItems)
                Remove(item);

            // Update
            for (var i = 0; i < _serializableItems.Count; i++)
            {
                if (Count <= i)
                    Add(_serializableItems[i]);
                else
                    this[i] = _serializableItems[i];
            }
        }
    
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NotifyList), true)]
    public class NotifyListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = property.FindPropertyRelative("_serializableItems");
            EditorGUI.PropertyField(position, list, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = property.FindPropertyRelative("_serializableItems");
            return EditorGUI.GetPropertyHeight(list, label, true);
        }
    }
#endif

}

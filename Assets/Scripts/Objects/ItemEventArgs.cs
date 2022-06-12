using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BLINDED_AM_ME.Objects
{
    public class ItemEventArgs : EventArgs
    {
        public object Item { get; set; }

        public ItemEventArgs(object item)
        {
            Item = item;
        }
    }

    public class ItemEventArgs<T> : ItemEventArgs
    {
        public new T Item
        {
            get => (T)base.Item;
            set => base.Item = value;
        }

        public ItemEventArgs(T item) : base(item) { }
    }

    public delegate void ItemEventHandler(object sender, ItemEventArgs args);
    public delegate void ItemEventHandler<T>(object sender, ItemEventArgs<T> args);

}

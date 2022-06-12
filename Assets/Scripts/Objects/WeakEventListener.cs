using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BLINDED_AM_ME.Objects
{
    /// <summary> Holds a weak reference to the subscriber </summary>
    [DebuggerNonUserCode]
    public sealed class WeakEventListener
    {
        public WeakReference TargetReference;
        public MethodInfo Method;

        public WeakEventListener(EventHandler callback)
        {
            TargetReference = new WeakReference(callback.Target, true);
            Method = callback.Method;
        }

        [DebuggerNonUserCode]
        public void Handle(object sender, EventArgs e)
        {
            var target = TargetReference.Target;
            if (target != null)
            {
                var callback = (Action<object, EventArgs>)Delegate.CreateDelegate(typeof(Action<object, EventArgs>), target, Method, true);
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }

        /// <summary> Sets Refrence to Target to null </summary>
        public void OptOut()
        {
            TargetReference.Target = null;
        }
    }

    /// <summary> Holds a weak reference to subscriber </summary>
    [DebuggerNonUserCode]
    public sealed class WeakEventListener<T>
    {
        public WeakReference TargetReference;
        public MethodInfo Method;

        public WeakEventListener(EventHandler<T> callback)
        {
            TargetReference = new WeakReference(callback.Target, true);
            Method = callback.Method;
        }

        [DebuggerNonUserCode]
        public void Handle(object sender, T e)
        {
            var target = TargetReference.Target;
            if (target != null)
            {
                var callback = (Action<object, T>)Delegate.CreateDelegate(typeof(Action<object, T>), target, Method, true);
                if (callback != null)
                {
                    callback(sender, e);
                }
            }
        }

        /// <summary> Sets Refrence to Target to null </summary>
        public void OptOut()
        {
            TargetReference.Target = null;
        }
    }

}

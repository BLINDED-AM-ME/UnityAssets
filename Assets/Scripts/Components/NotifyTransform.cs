using BLINDED_AM_ME.Extensions;
using BLINDED_AM_ME.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace BLINDED_AM_ME.Components
{
    [ExecuteInEditMode]
    public class NotifyTransform : MonoBehaviour2
    {
        private Vector3 _position = Vector3.zero;
        [HideInInspector]
        public Vector3 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        private Quaternion _rotation = Quaternion.identity;
        [HideInInspector]
        public Quaternion Rotation
        {
            get => _rotation;
            set => SetProperty(ref _rotation, value);
        }

        public event ItemEventHandler<Vector3> PositionChanged;
        public event ItemEventHandler<Quaternion> RotationChanged;

        public event ItemEventHandler<Transform> ChildAdded;
        public event ItemEventHandler<Transform> ChildRemoved;
        public event EventHandler ChildrenChanged;

        private NotifyList<Transform> _children = new NotifyList<Transform>();

        public NotifyTransform()
        {            
            _children.ItemAdded += (sender, args) => OnChildAdded(args.Item);
            _children.ItemRemoved += (sender, args) => OnChildRemoved(args.Item);
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            switch (propertyName)
            {
                case nameof(Position):
                    PositionChanged?.Invoke(this, new ItemEventArgs<Vector3>(Position));
                    break;

                case nameof(Rotation):
                    RotationChanged?.Invoke(this, new ItemEventArgs<Quaternion>(Rotation));
                    break;
            }

            base.OnPropertyChanged(propertyName);
        }

        protected override void OnEnable()
        {
            _children.Clear();
            foreach (Transform child in transform)
                _children.Add(child);

            base.OnEnable();
        }
        protected override void OnDisable()
        {
            _children.Clear();
            base.OnDisable();
        }

        protected override void Start()
        {
            Position = transform.position;
            Rotation = transform.rotation;
            base.Start();
        }
        protected override void Update()
        {
            // Da Fuq is this?
            if (transform.hasChanged)
            {
                transform.hasChanged = false;

                Position = transform.position;
                Rotation = transform.rotation;
            }

            base.Update();
        }

        protected override void OnTransformChildrenChanged()
        {
            // I need a F**king List
            var _currentChildren = new List<Transform>(transform.childCount);
            foreach (Transform child in transform)
                _currentChildren.Add(child);

            // Remove
            var deadItems = _children.Where(item => !_currentChildren.Contains(item)).ToList();
            foreach (var item in deadItems)
                _children.Remove(item); 

            // Add
            var newItems = _currentChildren.Where(item => !_children.Contains(item)).ToList();
            foreach (var item in newItems)
                _children.Add(item);

            base.OnTransformChildrenChanged();

            ChildrenChanged?.Invoke(this, EventArgs.Empty);
        }
        
        protected virtual void OnChildAdded(Transform child)
        {
            ChildAdded?.Invoke(this, new ItemEventArgs<Transform>(child));
        }
        protected virtual void OnChildRemoved(Transform child)
        {
            ChildRemoved?.Invoke(this, new ItemEventArgs<Transform>(child));
        }
    }
}

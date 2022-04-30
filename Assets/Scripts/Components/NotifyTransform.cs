using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace BLINDED_AM_ME
{
    [ExecuteInEditMode]
    public class NotifyTransform : MonoBehaviour
    {

        private Vector3 _position = Vector3.zero;

        public UnityEvent<Vector3> PositionChanged = new UnityEvent<Vector3>();

        private TransformDataModel _dataModel = new TransformDataModel();

        public NotifyTransform()
        {
            _dataModel.PositionChanged += DataModel_PositionChanged;
        }
        
        // Start is called before the first frame update
        void Start()
        {

        }
        
        // Update is called once per frame
        void Update()
        {
            _dataModel.Positon = transform.position;

            // Da Fuq is this?
            //if (transform.hasChanged)
            //{
            //    transform.hasChanged = false;

            //    if (_position != transform.position)
            //    {
            //        _position = transform.position;
            //        PositionChanged?.Invoke(_position);
            //    }
            //}
        }

        private void DataModel_PositionChanged(object sender, Vector3 e)
        {
            PositionChanged?.Invoke(e);
        }

        public class TransformDataModel : DataModel
        {
            private Vector3 _positon;
            public Vector3 Positon
            {
                get => _positon;
                set => SetProperty(ref _positon, value);
            }

            public event EventHandler<Vector3> PositionChanged;

            protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                switch (propertyName)
                {
                    case nameof(Positon):
                        PositionChanged.Invoke(this, Positon);
                        break;
                }

                base.OnPropertyChanged(propertyName);
            }

        }

    }
}

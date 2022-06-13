/*
 * Thanks to Aras Pranckevicius' MirrorReflection4
 * http://wiki.unity3d.com/index.php/MirrorReflection4 
 * Content is available under Creative Commons Attribution Share Alike.
 * http://creativecommons.org/licenses/by-sa/3.0/
 */

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.CompilerServices;
using System.Linq;
using System.ComponentModel;
using BLINDED_AM_ME.Objects;

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	[RequireComponent(typeof(PortalCameraController))]
	public class PortalView : MonoBehaviour2
	{
		public enum TextureSize
		{
			[InspectorName("32")]
			_32 = 32,
			[InspectorName("64")]
			_64 = 64,
			[InspectorName("128")]
			_128 = 128,
			[InspectorName("256")]
			_256 = 256,
			[InspectorName("512")]
			_512 = 512,
			[InspectorName("1024")]
			_1024 = 1024,
			[InspectorName("2048")]
			_2048 = 2048
		}

		[SerializeField]
		[SerializeProperty(nameof(TargetTextureSize))]
		private TextureSize _targetTextureSize = TextureSize._512;
		public TextureSize TargetTextureSize
		{
			get => _targetTextureSize;
			set => SetProperty(ref _targetTextureSize, value);
		}

		private RenderTexture _targetTexture;
		public RenderTexture TargetTexture
		{
			get => _targetTexture;
			set => SetProperty(ref _targetTexture, value);
		}

		//save to scene
		[SerializeField]
		[HideInInspector]
		private Material _targetMaterial;
		private Material TargetMaterial
		{
			get => _targetMaterial;
			set => SetProperty(ref _targetMaterial, value);
		}

		private Renderer _renderer;
		private PortalCameraController _cameraController;
		private WeakEventListener<PropertyChangedEventArgs> _cameraController_PropertyChangedListener;

		public PortalView() { }

        protected override void Awake()
        {
			_cameraController = GetComponent<PortalCameraController>();

			_renderer = GetComponent<Renderer>();
			_renderer.sharedMaterial = TargetMaterial;

            base.Awake();
        }
        protected override void OnEnable()
        {
			_cameraController_PropertyChangedListener?.OptOut();
			_cameraController_PropertyChangedListener = new WeakEventListener<PropertyChangedEventArgs>(CameraController_PropertyChanged);
			
			if (TryGetComponent(out PortalCameraController cameraController))
                cameraController.PropertyChanged += _cameraController_PropertyChangedListener.Handle;

            base.OnEnable();
        }
        protected override void OnDisable()
        {
			if (TryGetComponent(out PortalCameraController cameraController))
				cameraController.PropertyChanged -= _cameraController_PropertyChangedListener.Handle;

			_cameraController_PropertyChangedListener.OptOut();

			base.OnDisable();
        }
		
        protected override void Start()
        {
			TargetTexture = CreateTexture((int)TargetTextureSize);

			base.Start();
		}

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
			switch (propertyName)
            {
				case nameof(TargetTextureSize):
					var old = TargetTexture;
					TargetTexture = CreateTexture((int)TargetTextureSize);
					old?.Release();
					break;

				case nameof(TargetMaterial):
				case nameof(TargetTexture):

					if (_cameraController?.Camera != null)
						_cameraController.Camera.targetTexture = TargetTexture;

					if (TargetMaterial != null)
						TargetMaterial.mainTexture = TargetTexture;

                    break;
            }

            base.OnPropertyChanged(propertyName);
        }
		private void CameraController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
			var cameraController = (PortalCameraController)sender;
			switch (e.PropertyName)
			{
				case nameof(PortalCameraController.Camera):
					if (cameraController.Camera != null)
						cameraController.Camera.targetTexture = TargetTexture;
					break;
			}
		}

		protected override void OnWillRenderObject()
		{
			if (_renderer.sharedMaterial != TargetMaterial)
            {
				// create an instance
				var mat = _renderer.sharedMaterial;
				if (mat != null)
				{ 
					TargetMaterial = new Material(mat);
					TargetMaterial.name = $"{mat.name} ({TargetMaterial.GetInstanceID()})";
					_renderer.sharedMaterial = TargetMaterial;
                }
                else
                {
					TargetMaterial = null;
                }
			}
		}

		public RenderTexture CreateTexture(int size)
		{
			var x = new RenderTexture(size, size, 16, RenderTextureFormat.ARGB32);
			x.name = "__PortalRenderTexture" + x.GetInstanceID();
			x.hideFlags = HideFlags.DontSave;
			x.Create();
			return x;
		}
	}
}
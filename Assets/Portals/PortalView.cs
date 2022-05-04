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

namespace BLINDED_AM_ME
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class PortalView : MonoBehaviour 
	{
		public enum ClippingOptions
        {
			RelativeToSelf,
			RelativeToView
        }

		public Transform exit;
		public Camera    scoutCamera;
		public TextureSize targetTextureSize = TextureSize._512;
		
		public float           clippingDistance = 0.05f;
		public ClippingOptions clippingOption = ClippingOptions.RelativeToSelf;
		public Vector3         clippingNormal = Vector3.forward;

		private PortalViewDataModel _dataModel = new PortalViewDataModel();
		private MeshRenderer _meshRenderer;

		public PortalView()
        {
            _dataModel.PropertyChanged += DataModel_PropertyChanged;
        }

        private void DataModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
				case nameof(PortalViewDataModel.Material):
					_meshRenderer.sharedMaterial = _dataModel.Material;
					break;
            }
        }

        private void Reset()
        {
			Start();
        }

        private void Start()
        {
			_meshRenderer = GetComponent<MeshRenderer>();

            if (scoutCamera == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
					var child = transform.GetChild(i);
					var cam = child.GetComponent<Camera>();
					if (cam)
					{
						scoutCamera = cam;
						break;
					}
                }
            }
			
			if (scoutCamera == null)
            {
				var obj = new GameObject("ScoutCam", typeof(Camera));
				scoutCamera = obj.GetComponent<Camera>();
            }

			_dataModel.ScoutCamera = scoutCamera;
			_dataModel.TargetTextureSize = targetTextureSize;
		}

		private Vector4 _clippingPlane = new Vector4();
        private static bool _isRenderRecursion = false;
		public void OnWillRenderObject()
		{
			_dataModel.ScoutCamera = scoutCamera;
			_dataModel.Material = _meshRenderer.sharedMaterial;
			_dataModel.TargetTextureSize = targetTextureSize;

			Camera cam = Camera.current;
			if (!cam
			 || !exit
			 || !scoutCamera)
				return;

			// Safeguard from recursion    
			if (_isRenderRecursion)
				return;

			var selfToWorld = transform.localToWorldMatrix;
			var worldToSelf = transform.worldToLocalMatrix;
			var exitToWorld = exit.localToWorldMatrix;
			var camToWorld  = cam.transform.localToWorldMatrix;
			
			// this will make it depend on the points' position, rotation, and sorry also their scales
			// best make their scales 1 or equal

			// Scout = Cam's transform from World to Self to Exit
			scoutCamera.transform.SetPositionAndRotation(
				exitToWorld.MultiplyPoint3x4(
					worldToSelf.MultiplyPoint3x4(
						camToWorld.MultiplyPoint3x4(Vector3.zero))),
				Quaternion.LookRotation(
					exitToWorld.MultiplyVector(
						worldToSelf.MultiplyVector(
							camToWorld.MultiplyVector(Vector3.forward))),
					exitToWorld.MultiplyVector(
						worldToSelf.MultiplyVector(
							camToWorld.MultiplyVector(Vector3.up)))));

			// I don't know how this works it just does, I got lucky
			var worldToCam = cam.worldToCameraMatrix;
			switch (clippingOption)
			{
				case ClippingOptions.RelativeToSelf:
					{
						var normal = selfToWorld.MultiplyVector(-clippingNormal);
						_clippingPlane = worldToCam.MultiplyVector(normal);
						_clippingPlane.w = -Vector3.Dot(
								worldToCam.MultiplyPoint3x4(
									selfToWorld.MultiplyPoint3x4(Vector3.zero) + normal * clippingDistance),
								_clippingPlane);
					}
                    break;

				case ClippingOptions.RelativeToView:
					{
						_clippingPlane = Vector3.forward;
						_clippingPlane.w = -Vector3.Dot(
								worldToCam.MultiplyPoint3x4(
									selfToWorld.MultiplyPoint3x4(Vector3.zero) + camToWorld.MultiplyVector(Vector3.forward) * clippingDistance),
								_clippingPlane);
					}
					break;

			}

			scoutCamera.projectionMatrix = cam.CalculateObliqueMatrix(_clippingPlane);
			scoutCamera.enabled = false;// make it manual

			_isRenderRecursion = true;
			scoutCamera.Render();
			_isRenderRecursion = false;
		}

		private class PortalViewDataModel : DataModel
        {
			private Camera _scoutCamera;
			public Camera ScoutCamera
			{
				get => _scoutCamera;
				set => SetProperty(ref _scoutCamera, value);
			}
			
			private TextureSize _targetTextureSize = TextureSize._32;
			public TextureSize TargetTextureSize
			{
				get => _targetTextureSize;
				set => SetProperty(ref _targetTextureSize, value);
			}

			private RenderTexture _targetTexture;
			public RenderTexture TargetTexture
			{
                get => _targetTexture;
				set
				{
					var old = _targetTexture;
					if (SetProperty(ref _targetTexture, value))
						if(old != null)
							old.Release();
				}
			}
			
			private Material _material;
			public Material Material
			{
				get => _material;
				set => SetProperty(ref _material, value);
			}

			public PortalViewDataModel()
            {

            }

            protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
				switch (propertyName)
                {
					case nameof(TargetTextureSize):
						var targetTexture = new RenderTexture((int)TargetTextureSize, (int)TargetTextureSize, 16, RenderTextureFormat.ARGB32);
						targetTexture.name = "__PortalRenderTexture" + targetTexture.GetInstanceID();
						targetTexture.hideFlags = HideFlags.DontSave;
						targetTexture.Create();
						TargetTexture = targetTexture;
						break;
					
					case nameof(Material):
						if (Material == null)
							break;

						// create an instance
						var name = _material.name.Split('-').First();
						_material = new Material(_material);
						_material.name = name + _material.GetInstanceID(); 
						
						if (TargetTexture != null)
							Material.mainTexture = TargetTexture;

						break;

					case nameof(ScoutCamera):
					case nameof(TargetTexture):
						if (TargetTexture == null)
							break;

                        if (ScoutCamera != null)
                            ScoutCamera.targetTexture = TargetTexture;

						if (Material != null)
							Material.mainTexture = TargetTexture;

						break;
                }

                base.OnPropertyChanged(propertyName);
            }
		}
	}
}
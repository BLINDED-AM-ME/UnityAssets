using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	public class PortalCameraController : MonoBehaviour2
	{
		public enum ClippingOptions
		{
			RelativeToObject,
			RelativeToCamera
		}

		[SerializeField]
		[SerializeProperty(nameof(Exit))]
		private Transform _exit;
		public Transform Exit
		{
			get => _exit;
			set => SetProperty(ref _exit, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(Camera))]
		private Camera _camera;
		public Camera Camera
		{
			get => _camera;
			set => SetProperty(ref _camera, value);
		}

		public float clippingDistance = 0.05f;
		public ClippingOptions clippingOption = ClippingOptions.RelativeToObject;
		public Vector3 clippingNormal = Vector3.forward;

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Camera):
					Camera.enabled = false;
					break;
			}
			base.OnPropertyChanged(propertyName);
		}

		private Vector4 _clippingPlane = new Vector4();
		private static bool _isRenderRecursion = false;
		protected override void OnWillRenderObject()
		{
			// Safeguard from recursion    
			if (_isRenderRecursion)
				return;

			Camera cam = Camera.current;
			if (!cam
			 || !Exit
			 || !Camera)
				return;

			var selfToWorld = transform.localToWorldMatrix;
			var worldToSelf = transform.worldToLocalMatrix;
			var exitToWorld = Exit.localToWorldMatrix;
			var camToWorld = cam.transform.localToWorldMatrix;

			// this will make it depend on the points' position, rotation, and sorry also their scales
			// best make their scales 1 or equal

			// Scout = Cam's transform from World to Self to Exit
			Camera.transform.SetPositionAndRotation(
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
				case ClippingOptions.RelativeToObject:
					{
						var normal = selfToWorld.MultiplyVector(-clippingNormal);
						_clippingPlane = worldToCam.MultiplyVector(normal);
						_clippingPlane.w = -Vector3.Dot(
								worldToCam.MultiplyPoint3x4(
									selfToWorld.MultiplyPoint3x4(Vector3.zero) + normal * clippingDistance),
								_clippingPlane);
					}
					break;

				case ClippingOptions.RelativeToCamera:
					{
						_clippingPlane = Vector3.forward;
						_clippingPlane.w = -Vector3.Dot(
								worldToCam.MultiplyPoint3x4(
									selfToWorld.MultiplyPoint3x4(Vector3.zero) + camToWorld.MultiplyVector(Vector3.forward) * clippingDistance),
								_clippingPlane);
					}
					break;

			}

			Camera.projectionMatrix = cam.CalculateObliqueMatrix(_clippingPlane);
			Camera.enabled = false;// make it manual

			_isRenderRecursion = true;
			Camera.Render();
			_isRenderRecursion = false;
		}
	}
}
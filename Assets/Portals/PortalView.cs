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
		
		public float           clippingDistance = 0.05f;
		public ClippingOptions clippingOption = ClippingOptions.RelativeToView;
		public Vector3         clippingNormal = -Vector3.forward;

		private Vector4 _clippingPlane = new Vector4();
		private static bool _isRenderRecursion = false;
		public void OnWillRenderObject()
		{
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

		//_portalTexture = new RenderTexture(portalTextureSize, portalTextureSize, 16, RenderTextureFormat.ARGB32);
		//_portalTexture.name = "__PortalRenderTexture" + GetInstanceID();
		//_portalTexture.hideFlags = HideFlags.DontSave;
		//		_portalTexture.Create();
	}
}
/*
 * Thanks to Aras Pranckevicius' MirrorReflection4
 * http://wiki.unity3d.com/index.php/MirrorReflection4 
 * Content is available under Creative Commons Attribution Share Alike.
 * http://creativecommons.org/licenses/by-sa/3.0/
 */

//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Youtube stuff: https://www.youtube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software without restriction, including without limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

using UnityEngine;
using System.Collections;

namespace BLINDED_AM_ME{

	[ExecuteInEditMode]
	public class PortalView : MonoBehaviour {

		public Transform      pointB;
		public Camera         scoutCamera;
		public Vector3        faceNormal = Vector3.forward; // relative to self

		public int            portalTextureSize = 256;
		public float          clipPlaneOffset = 0.07f;

		private RenderTexture _portalTexture = null;
		private int           _oldPortalTextureSize = 0;

		private static bool   _isInsideRendering = false;

		// Use this for initialization
		void Start () {
		
		}

		public void OnWillRenderObject()
		{

			if(!enabled || !scoutCamera || !pointB)
				return;

			Camera cam = Camera.current;
			if( !cam )
				return;

			var rend = GetComponent<Renderer>();
			if (!rend || !rend.sharedMaterial || !rend.enabled)
				return;

			CreateNeededObjects();

			if(!_portalTexture.IsCreated())
				return;

			// Safeguard from recursive reflections.        
			if( _isInsideRendering)
				return;
			_isInsideRendering = true;



			Matrix4x4 self_toWorld = transform.localToWorldMatrix;
			Matrix4x4 self_toLocal = transform.worldToLocalMatrix;
			Matrix4x4 pointB_toWorld = pointB.localToWorldMatrix;


			// this will make it depend on the points' position, rotation, and sorry also their scales
			// best make their scales 1 or equal
			scoutCamera.transform.position = pointB_toWorld.MultiplyPoint(self_toLocal.MultiplyPoint(cam.transform.position));
			scoutCamera.transform.rotation = Quaternion.LookRotation(
				pointB_toWorld.MultiplyVector(self_toLocal.MultiplyVector(cam.transform.forward)),
				pointB_toWorld.MultiplyVector(self_toLocal.MultiplyVector(cam.transform.up)));


			// I don't know how this works it just does, I got lucky
			Vector4   clipPlane = CameraSpacePlane( cam, transform.position, self_toWorld.MultiplyVector(faceNormal), -1.0f );
			Matrix4x4 projection = cam.CalculateObliqueMatrix(clipPlane);
			scoutCamera.projectionMatrix = projection;

			if(scoutCamera.enabled) // make it manual
				scoutCamera.enabled = false;

			scoutCamera.Render();


			_isInsideRendering = false;

		}
			
		void OnDisable()
		{
			if( _portalTexture ) {
				_portalTexture.Release();
				CustomDestroy(_portalTexture);
			}
		}


		private void CreateNeededObjects()
		{
			
			// Reflection render texture
			if(portalTextureSize > 1)
			if( !_portalTexture || _oldPortalTextureSize != portalTextureSize)
			{

				if( _portalTexture ) {
					_portalTexture.Release();
					CustomDestroy(_portalTexture);
				}

				_portalTexture = new RenderTexture( portalTextureSize, portalTextureSize, 16, RenderTextureFormat.ARGB32);
				_portalTexture.name = "__PortalRenderTexture" + GetInstanceID();
				_portalTexture.hideFlags = HideFlags.DontSave;
				_portalTexture.Create();

				scoutCamera.targetTexture = _portalTexture;


				_oldPortalTextureSize = portalTextureSize;

			}

			Material[] materials = GetComponent<Renderer>().sharedMaterials;
			foreach( Material mat in materials ) {
				if( mat.HasProperty("_PortalTex") )
					mat.SetTexture( "_PortalTex", _portalTexture);
			}
	       
		}

		// Aras Pranckevicius' MirrorReflection4
		// http://wiki.unity3d.com/index.php/MirrorReflection4 
		// Given position/normal of the plane, calculates plane in camera space.
		private Vector4 CameraSpacePlane (Camera cam, Vector3 pos, Vector3 normal, float sideSign)
		{
			Vector3 offsetPos = pos + normal * -clipPlaneOffset;
			Matrix4x4 m = cam.worldToCameraMatrix;
			Vector3 cpos = m.MultiplyPoint( offsetPos );
			Vector3 cnormal = m.MultiplyVector( normal ).normalized * sideSign;
			return new Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
		}
			
		private void CustomDestroy(Object target){

			#if UNITY_EDITOR
			if(!Application.isPlaying)
				DestroyImmediate( target );
			else
				Destroy( target );
			#else
			Destroy( target );
			#endif

		}
	}
}
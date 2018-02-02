
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

namespace BLINDED_AM_ME._2D{

	[ExecuteInEditMode]
	public class Parallax2D : MonoBehaviour {

		[System.Serializable]
		public struct TwoDeeLayer{

			[Range(0.0f, 1.0f)]
			public float move_multipler;
			public float zPos;
			public Transform layer;

		}

		public Transform     _targetCam;
		public Vector2       _offset = Vector2.zero;
		public TwoDeeLayer[] _layers;


		void Reset(){

			_layers = new TwoDeeLayer[transform.childCount];

			for(int i=0; i< transform.childCount; i++){
				_layers[i].layer = transform.GetChild(i);
				_layers[i].move_multipler = (float)(i+1)/((float)transform.childCount+1.0f);
				_layers[i].zPos = (i+1)*1.0f;

			}

		}

		// Use this for initialization
		void Start () {

			if( !_targetCam){
				_targetCam = Camera.main.transform;
			}

		}
			
		void LateUpdate(){

			if( !_targetCam){
				_targetCam = Camera.main.transform;
				return;
			}

			AdjustLayers(_targetCam.position);

		}

//		public void OnWillRenderObject()
//		{
//			if(!enabled)
//				return;
//
//			Camera cam = Camera.current;
//			if( !cam )
//				return;
//
//			AdjustLayers(cam.transform.position);
//
//		}

		void AdjustLayers(Vector3 viewPoint){

			viewPoint += (Vector3) _offset;

			Vector3 displacement = viewPoint - transform.position;
			Vector3 layerSpot = Vector3.zero;

			for(int i=0; i<_layers.Length; i++){

				layerSpot = displacement * _layers[i].move_multipler;
				layerSpot.z = _layers[i].zPos;

				_layers[i].layer.localPosition = layerSpot;

			}

		}
	}

}
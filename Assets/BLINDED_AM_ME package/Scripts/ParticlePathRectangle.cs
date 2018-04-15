
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
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME{

	[ExecuteInEditMode]
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticlePathRectangle : MonoBehaviour {

		public bool isSmooth = false;
		public bool isFlat   = true;
		public bool hasRandomStartingPoints = false;

		[Range(0.0f, 5.0f)]
		public float pathWidth = 0.0f;

		public Vector2 rectangleSize = new Vector2(10.0f, 5.0f);
		public bool    isRectangleSizeUpdating = false;

		private ParticleSystem.Particle[] _particle_array;
		private ParticleSystem            _particle_system;
		private Path                      _path = new Path();

		private int _numParticles;


		#if UNITY_EDITOR
		void Reset(){
			Start();
		}

		void OnValidate(){
			Calculate_The_Four_Corners();
		}
		#endif

		void Start(){

			_particle_system = GetComponent<ParticleSystem>();
			_particle_array = new ParticleSystem.Particle[_particle_system.main.maxParticles];

			Calculate_The_Four_Corners();

		}


		void LateUpdate () {

			if(_particle_array == null)
				Start();

			if(isRectangleSizeUpdating)
				Calculate_The_Four_Corners();

			_numParticles = _particle_system.GetParticles(_particle_array);

			if(_numParticles > 0){

				for(int i=0; i<_numParticles; i++){

					ParticleSystem.Particle obj = _particle_array[i];

					// This made it based on the particle lifetime
//					float normalizedLifetime = (1.0f - obj.remainingLifetime / obj.startLifetime);
//
//					if(hasRandomStartingPoints){
//						normalizedLifetime += Get_Value_From_Random_Seed_0t1(obj.randomSeed, 100.0f);
//						normalizedLifetime = normalizedLifetime % 1.0f;
//					}
//
//					Path_Point axis = _path.GetPathPoint(_path.TotalDistance * normalizedLifetime, isSmooth);


					// This made it based on the paritcle speed
					float dist = (obj.startLifetime - obj.remainingLifetime) * obj.velocity.magnitude;
					if(hasRandomStartingPoints)
						dist += Get_Value_From_Random_Seed_0t1(obj.randomSeed, 100.0f) * _path.TotalDistance;
					dist = dist % _path.TotalDistance;

					Path_Point axis = _path.GetPathPoint(dist, isSmooth);


					Vector2    offset = Vector2.zero;

					if(pathWidth > 0){
						offset = Math_Functions.AngleToVector2D(obj.randomSeed % 360.0f);
						offset *= Get_Value_From_Random_Seed_0t1(obj.randomSeed, 150.0f) * pathWidth;
					}

					_particle_array[i].position = axis.point + 
						(isFlat ? Vector3.zero :  axis.right * offset.x) +
						(                         axis.up    * offset.y);

					_particle_array[i].velocity = axis.forward * _particle_array[i].velocity.magnitude;

				}

				_particle_system.SetParticles(_particle_array, _numParticles);

			}


		}

		private float Get_Value_From_Random_Seed_0t1(float seed, float converter){
			return (seed % converter) / converter;
		}


		private void Calculate_The_Four_Corners(){

			_path.SetPoints(
				new Vector3[]{
					(-Vector3.right * (rectangleSize.x * 0.5f)) + ( -Vector3.up * (rectangleSize.y * 0.5f)),
					(-Vector3.right * (rectangleSize.x * 0.5f)) + (  Vector3.up * (rectangleSize.y * 0.5f)),
					( Vector3.right * (rectangleSize.x * 0.5f)) + (  Vector3.up * (rectangleSize.y * 0.5f)),
					( Vector3.right * (rectangleSize.x * 0.5f)) + ( -Vector3.up * (rectangleSize.y * 0.5f))
				},
				new Vector3[]{
					Vector3.Lerp(-Vector3.right,-Vector3.up, 0.5f),
					Vector3.Lerp(-Vector3.right, Vector3.up, 0.5f),
					Vector3.Lerp( Vector3.right, Vector3.up, 0.5f),
					Vector3.Lerp( Vector3.right,-Vector3.up, 0.5f)
				},
				true);

		}
			


		private void OnDrawGizmosSelected()
		{

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(
				transform.TransformPoint((-Vector3.right * (rectangleSize.x * 0.5f)) + (-Vector3.up * (rectangleSize.y * 0.5f))),
				transform.TransformPoint((-Vector3.right * (rectangleSize.x * 0.5f)) + ( Vector3.up * (rectangleSize.y * 0.5f)))
			);
			Gizmos.DrawLine(
				transform.TransformPoint((-Vector3.right * (rectangleSize.x * 0.5f)) + ( Vector3.up * (rectangleSize.y * 0.5f))),
				transform.TransformPoint(( Vector3.right * (rectangleSize.x * 0.5f)) + ( Vector3.up * (rectangleSize.y * 0.5f)))
			);
			Gizmos.DrawLine(
				transform.TransformPoint(( Vector3.right * (rectangleSize.x * 0.5f)) + ( Vector3.up * (rectangleSize.y * 0.5f))),
				transform.TransformPoint(( Vector3.right * (rectangleSize.x * 0.5f)) + (-Vector3.up * (rectangleSize.y * 0.5f)))
			);
			Gizmos.DrawLine(
				transform.TransformPoint(( Vector3.right * (rectangleSize.x * 0.5f)) + (-Vector3.up * (rectangleSize.y * 0.5f))),
				transform.TransformPoint((-Vector3.right * (rectangleSize.x * 0.5f)) + (-Vector3.up * (rectangleSize.y * 0.5f)))
			);


		}

	}
}

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
	[RequireComponent(typeof(Path_Comp))]
	public class ParticlePathFlow : MonoBehaviour {

		public bool isPathUpdating = false;

		[Range(0.0f, 5.0f)]
		public float pathWidth = 0.0f;


		private ParticleSystem.Particle[] _particle_array;
		private ParticleSystem            _particle_system;
		private Path_Comp 				  _path_comp;

		private int _numParticles;


	#if UNITY_EDITOR
		void Reset(){
			Start();
		}
	#endif
	
		void Start(){
			
			_path_comp = GetComponent<Path_Comp>();
			_particle_system = GetComponent<ParticleSystem>();
			_particle_array = new ParticleSystem.Particle[_particle_system.main.maxParticles];
		}


		void LateUpdate () {

			if(_particle_array == null){
			
				Start();
				_path_comp.Update_Path();
			
			}else if(isPathUpdating){

				_path_comp.Update_Path();

			}



			_numParticles = _particle_system.GetParticles(_particle_array);

			if(_numParticles > 0){

				for(int i=0; i<_numParticles; i++){

					ParticleSystem.Particle obj = _particle_array[i];
					Path_Point              axis = _path_comp.GetPathPoint(_path_comp.TotalDistance * (1.0f - obj.remainingLifetime / obj.startLifetime));
					Vector2                 offset = Math_Functions.AngleToVector2D(obj.randomSeed % 360.0f);

					offset *= (((float) obj.randomSeed % 100.0f) / 100.0f) * pathWidth;

					_particle_array[i].position = axis.point + 
						(axis.right * offset.x) +
						(axis.up    * offset.y);
					
					_particle_array[i].velocity = axis.forward * _particle_array[i].velocity.magnitude;

				}
					
				_particle_system.SetParticles(_particle_array, _numParticles);

			}


		}			

	}
}
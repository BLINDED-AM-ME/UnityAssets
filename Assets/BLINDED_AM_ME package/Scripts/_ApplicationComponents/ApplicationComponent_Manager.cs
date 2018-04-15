
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


namespace BLINDED_AM_ME._ApplicationComponents{

	/// <summary>
	/// Meant for loading at start up. There were more scripts for Save data, Achievements, and Social profiles 
	/// </summary>
	public class ApplicationComponent_Manager : MonoBehaviour {

		public class ApplicationComponent : MonoBehaviour {

			public delegate void CallBack();
			public virtual void Initialize(CallBack callback){}

		}

		public float PercentComplete{
			get{
				if(_progress == 0)
					return 0.0f;
				else
					return (float) _progress/(float) _numberOfProcesses * 100.0f;
			}
		}

		private int _progress = 0;
		private int _numberOfProcesses = 1;


		// Use this for initialization
		IEnumerator Start () {

			DontDestroyOnLoad(gameObject);

			ApplicationComponent[] comps = GetComponents<ApplicationComponent>(); // num of processes

			_numberOfProcesses = comps.Length;

			for(int i=0; i<_numberOfProcesses; i++){

				comps[i].Initialize(delegate { 
					_progress++;
				});

				while(_progress <= i)
					yield return null; // wait for the current comps[i].Initialize(callback)
			}
				
		}
						
	}

}
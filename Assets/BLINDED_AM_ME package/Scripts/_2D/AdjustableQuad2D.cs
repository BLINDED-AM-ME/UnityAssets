
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
using BLINDED_AM_ME._ScriptHelper;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME._2D
{

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(PolygonCollider2D))]
	public class AdjustableQuad2D : MonoBehaviour {

		#if UNITY_EDITOR

		public bool useWorldUvPositions = true;

		[HideInInspector]
		public bool isEditMode = true;

		// Mesh Values
		private List<Vector3>   _vertices  = new List<Vector3>();
		private List<Vector3>   _normals   = new List<Vector3>();
		private List<Vector2>   _uvs       = new List<Vector2>();
		private List<int>       _triangles = new List<int>();
		private List<List<int>> _subIndices = new List<List<int>>();

		public void Reset(){

			ShapeIt();

			if(transform.childCount != 4){

				// destroy the children
				while(transform.childCount > 0){

					Transform obj = transform.GetChild(0);
					obj.parent = null;
					DestroyImmediate(obj.gameObject);
				}

				for(int i=0; i<_vertices.Count; i++){
					Transform obj = new GameObject("vert("+i+")").transform;
					obj.SetParent(transform);
					obj.localPosition = _vertices[i];
					obj.gameObject.AddComponent<SelectableSphereGizmo>();
					obj.gameObject.isStatic = true;
				}
			}

		}

		private void OnDrawGizmos(){

			if(!Application.isPlaying && Selection.activeTransform != null){
				if(Selection.activeTransform == this.transform || Selection.activeTransform.parent == this.transform)
					ShapeIt();
			}


		}
			


		private void ShapeIt(){

			// because it messes it up
			//Quaternion oldRotation = transform.rotation;
			//transform.rotation = Quaternion.identity;


			Craft(); // make segments
			Apply(); // apply values

			//transform.rotation = oldRotation;

		}

		public void Craft(){

			Vector3[] fourCorners = new Vector3[]{
				new Vector3(-0.5f,-0.5f,0),
				new Vector3(-0.5f,0.5f,0),
				new Vector3(0.5f,0.5f,0),
				new Vector3(0.5f,-0.5f,0)

			};

			if(transform.childCount == 4){
				fourCorners[0] = transform.GetChild(0).localPosition;
				fourCorners[1] = transform.GetChild(1).localPosition;
				fourCorners[2] = transform.GetChild(2).localPosition;
				fourCorners[3] = transform.GetChild(3).localPosition;
			}


			_vertices.Clear();
			_uvs.Clear();
			_normals.Clear();
			_triangles.Clear();

			AddTriangle(new Vector3[]{
				fourCorners[0],
				fourCorners[1],
				fourCorners[2]

			}, useWorldUvPositions ? 
				new Vector2[]{
					(Vector2) transform.TransformPoint(fourCorners[0]),
					(Vector2) transform.TransformPoint(fourCorners[1]),
					(Vector2) transform.TransformPoint(fourCorners[2])
				} : new Vector2[]{
					fourCorners[0],
					fourCorners[1],
					fourCorners[2]
				});

			AddTriangle(new Vector3[]{
				fourCorners[2],
				fourCorners[3],
				fourCorners[0]

			}, useWorldUvPositions ? 
				new Vector2[]{
					(Vector2) transform.TransformPoint(fourCorners[2]),
					(Vector2) transform.TransformPoint(fourCorners[3]),
					(Vector2) transform.TransformPoint(fourCorners[0])
				} : new Vector2[]{
					fourCorners[2],
					fourCorners[3],
					fourCorners[0]
				});

		}

		private void AddTriangle(Vector3[] verts, Vector2[] uvs){

			for(int i=0; i<3; i++){

				// is this new?
				int index = -1;

				for(int iterator=0; iterator<_vertices.Count; iterator++){
					if(_vertices[iterator] == verts[i] &&
						_normals[iterator] == _normals[i] &&
						_uvs[iterator] == uvs[i]){
						index = iterator;
						break;
					}
				}

				// it is
				if(index == -1){
					_triangles.Add(_vertices.Count);
					_vertices.Add(verts[i]);
					_normals.Add(Vector3.back);
					_uvs.Add(uvs[i]);

				}else{ // it is not
					_triangles.Add(index);
				}

			}
		}


		public void Apply(){


			Mesh _mesh = new Mesh();
			_mesh.name =  "Mesh "+ Selection.activeInstanceID;
			_mesh.vertices  = _vertices.ToArray();
			_mesh.triangles = _triangles.ToArray();
			_mesh.normals   = _normals.ToArray();
			_mesh.uv        = _uvs.ToArray();

			_subIndices.Clear();
			_subIndices.Add(new List<int>());
			for(int i=0; i<_triangles.Count; i++)
				_subIndices[0].Add(_triangles[i]);


			_mesh.subMeshCount = 1;
			_mesh.SetIndices(_subIndices[0].ToArray(), MeshTopology.Triangles, 0);	


			GetComponent<MeshFilter>().mesh = _mesh;

			if(GetComponent<MeshFilter>().sharedMesh == null)
				Debug.Log("Hello World");

			GetComponent<PolygonCollider2D>().points = new Vector2[]{
				_vertices[0],
				_vertices[1],
				_vertices[2],
				_vertices[3]
			};

		}
			

		#endif
	}
}

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

namespace BLINDED_AM_ME{

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(Path_Comp))]
	public class Spline3D : MonoBehaviour {


		public bool   removeDuplicateVertices = false; // removes doubles
		public Mesh   segment_sourceMesh;

		private float _segment_length;
		private float _segment_MinZ;
		private float _segment_MaxZ;

		private Path_Comp _path;
		private Transform  _helpTransform1;
		private Transform  _helpTransform2;

		private Mesh_Maker _maker = new Mesh_Maker();

	#if UNITY_EDITOR
		public enum LightmapUnwrapping{
			UseFirstUvSet,
			DefaultUnwrapParam
		}
		public LightmapUnwrapping lightmapUnwrapping = LightmapUnwrapping.UseFirstUvSet;
	#endif

		/// <summary>
		/// You can call this during runtime and in the editor
		/// </summary>
		public void ShapeIt(){

			if(segment_sourceMesh == null){
				Debug.LogError("missing source mesh");
				return;
			}


			_helpTransform1 = new GameObject("_helpTransform1").transform;
			_helpTransform2 = new GameObject("_helpTransform2").transform;

			// because it messes it up
			Quaternion oldRotation = transform.rotation;
			transform.rotation = Quaternion.identity;


			_maker = new Mesh_Maker();
			ScanSourceMesh();
			Craft(); // make segments
			Apply(); // apply values

			transform.rotation = oldRotation;

		#if UNITY_EDITOR
			if(!Application.isPlaying){
				DestroyImmediate(_helpTransform1.gameObject);
				DestroyImmediate(_helpTransform2.gameObject);
			}else{
				Destroy(_helpTransform1.gameObject);
				Destroy(_helpTransform2.gameObject);
			}
		#else
			Destroy(_helpTransform1.gameObject);
			Destroy(_helpTransform2.gameObject);
		#endif
		}

		private void Craft(){

			_path = GetComponent<Path_Comp>();
			Path_Point pointA = _path.GetPathPoint(0.0f);
			Path_Point pointB = pointA;
		

			for(float dist=0.0f; dist<_path._path.TotalDistance; dist+=_segment_length){
				
				pointB = _path.GetPathPoint(Mathf.Clamp(dist + _segment_length,0,_path._path.TotalDistance));

				_helpTransform1.rotation = Quaternion.LookRotation(pointA.forward, pointA.up);
				_helpTransform1.position = transform.TransformPoint(pointA.point);

				_helpTransform2.rotation = Quaternion.LookRotation(pointB.forward, pointB.up);
				_helpTransform2.position = transform.TransformPoint(pointB.point);

				Add_Segment();

				pointA = pointB;
			}

		}

		private void Add_Segment(){

			int[] indices;

			// go throughout the submeshes
			for(int sub=0; sub<segment_sourceMesh.subMeshCount; sub++){
				indices = segment_sourceMesh.GetIndices(sub);
				for(int i=0; i<indices.Length; i+=3){


					AddTriangle(new int[]{
						indices[i],
	                    indices[i+1],
	                    indices[i+2]
					},sub);
				}
			}
		}

		private void AddTriangle( int[] indices, int submesh){

			// vertices
			Vector3[] verts = new Vector3[3]{
				segment_sourceMesh.vertices[indices[0]],
				segment_sourceMesh.vertices[indices[1]],
				segment_sourceMesh.vertices[indices[2]]
			};
			// normals
			Vector3[] norms = new Vector3[3]{
				segment_sourceMesh.normals[indices[0]],
				segment_sourceMesh.normals[indices[1]],
				segment_sourceMesh.normals[indices[2]]
			};
			// uvs
			Vector2[] uvs = new Vector2[3]{
				segment_sourceMesh.uv[indices[0]],
				segment_sourceMesh.uv[indices[1]],
				segment_sourceMesh.uv[indices[2]]
			};
			// tangent
			Vector4[] tangents = new Vector4[3]{
				segment_sourceMesh.tangents[indices[0]],
				segment_sourceMesh.tangents[indices[1]],
				segment_sourceMesh.tangents[indices[2]]
			};

			// apply offset
			float lerpValue = 0.0f;
			Vector3 pointA, pointB;
			Vector3 normA, normB;
			Vector4 tangentA, tangentB;
			Matrix4x4 localToWorld_A = _helpTransform1.localToWorldMatrix;
			Matrix4x4 localToWorld_B = _helpTransform2.localToWorldMatrix;
			Matrix4x4 worldToLocal =   transform.worldToLocalMatrix;
			for(int i=0; i<3; i++){

				lerpValue = Math_Functions.Value_from_another_Scope(verts[i].z, _segment_MinZ, _segment_MaxZ, 0.0f, 1.0f);
				verts[i].z = 0.0f;
					
				pointA = localToWorld_A.MultiplyPoint(verts[i]); // to world
				pointB = localToWorld_B.MultiplyPoint(verts[i]);

				verts[i] = worldToLocal.MultiplyPoint(Vector3.Lerp(pointA, pointB,lerpValue)); // to local

				normA = localToWorld_A.MultiplyVector(norms[i]);
				normB = localToWorld_B.MultiplyVector(norms[i]);

				norms[i] = worldToLocal.MultiplyVector(Vector3.Lerp(normA, normB, lerpValue));

				tangentA = localToWorld_A.MultiplyVector(tangents[i]);
				tangentB = localToWorld_B.MultiplyVector(tangents[i]);

				tangents[i] = worldToLocal.MultiplyVector(Vector3.Lerp(tangentA, tangentB, lerpValue));

			}

			_maker.AddTriangle(verts, uvs, norms, tangents, submesh);

		}


		private void ScanSourceMesh(){

			float min_z = 0.0f, max_z = 0.0f;

			// find length
			for(int i=0; i<segment_sourceMesh.vertexCount;i++){

				Vector3 vert = segment_sourceMesh.vertices[i];
				if(vert.z < min_z)
					min_z = vert.z;

				if(vert.z > max_z)
					max_z = vert.z;
			}

			_segment_MinZ = min_z;
			_segment_MaxZ = max_z;
			_segment_length = max_z - min_z;

		}

		private void Apply(){


			if(removeDuplicateVertices){
				_maker.RemoveDoubles();
			}

		#if UNITY_EDITOR
			if(!Application.isPlaying){

				switch(lightmapUnwrapping){
				case LightmapUnwrapping.UseFirstUvSet:
					GetComponent<MeshFilter>().mesh = _maker.GetMesh();
					break;
				case LightmapUnwrapping.DefaultUnwrapParam:
					GetComponent<MeshFilter>().mesh = _maker.GetMesh_GenerateSecondaryUVSet();
					break;
				default:
					GetComponent<MeshFilter>().mesh = _maker.GetMesh();
					break;
				}

			}else
				GetComponent<MeshFilter>().mesh = _maker.GetMesh();
		#else
			GetComponent<MeshFilter>().mesh = _maker.GetMesh();
		#endif

		}

	}
}
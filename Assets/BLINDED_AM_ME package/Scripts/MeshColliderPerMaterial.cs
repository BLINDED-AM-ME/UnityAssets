
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

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class MeshColliderPerMaterial : MonoBehaviour {

		#if UNITY_EDITOR


		public void Button_MakeIt(){

			Mesh         _mesh = GetComponent<MeshFilter>().sharedMesh;
			MeshRenderer _renderer = GetComponent<MeshRenderer>(); 

			Mesh[] _made_meshes = new Mesh[_mesh.subMeshCount];

			// go through the sub indices
			for(int sub=0; sub<_mesh.subMeshCount; sub++){

				Mesh_Maker maker = new Mesh_Maker();
				int[] trinagles = _mesh.GetTriangles(sub);

				// go through the triangles
				for(int i=0; i<trinagles.Length; i+=3){

					maker.AddTriangle(new Vector3[]{
						_mesh.vertices[trinagles[i]], _mesh.vertices[trinagles[i+1]], _mesh.vertices[trinagles[i+2]]
					}, new Vector2[]{
						_mesh.uv[trinagles[i]], _mesh.uv[trinagles[i+1]], _mesh.uv[trinagles[i+2]]
					}, new Vector3[]{
						_mesh.normals[trinagles[i]], _mesh.normals[trinagles[i+1]], _mesh.normals[trinagles[i+2]]
					},0);
				}

				maker.RemoveDoubles();

				_made_meshes[sub] = maker.GetMesh();
			}



			// too many
			while(transform.childCount > _mesh.subMeshCount){
				Transform obj = transform.GetChild(transform.childCount-1);
				obj.parent = null;
				DestroyImmediate(obj.gameObject);
			}

			// too little
			while(transform.childCount < _mesh.subMeshCount){
				Transform obj = new GameObject("child").transform;
				obj.SetParent(transform);
				obj.localPosition = Vector3.zero;
				obj.localRotation = Quaternion.identity;
				obj.gameObject.AddComponent<MeshCollider>();
			}
				
			for(int i=0; i<transform.childCount; i++){
				Transform obj = transform.GetChild(i);

				obj.gameObject.name = _renderer.sharedMaterials[i].name;

				_made_meshes[i].name = obj.gameObject.name;

				obj.GetComponent<MeshCollider>().sharedMesh = _made_meshes[i];

			}

		}

	
		#endif
	}
}
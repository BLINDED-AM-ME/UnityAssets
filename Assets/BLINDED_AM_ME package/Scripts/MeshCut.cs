
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

	public class MeshCut{

		private static Mesh_Maker _leftSide = new Mesh_Maker();
		private static Mesh_Maker _rightSide = new Mesh_Maker();


		private static Plane _blade;
		private static Mesh  _victim_mesh;

		// capping stuff
		private static List<Vector3> _new_vertices = new List<Vector3>();

		private static int _capMatSub = 1;

		/// <summary>
		/// Cut the specified victim
		/// </summary>
		public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial){

			// set the blade relative to victim
			_blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
				victim.transform.InverseTransformPoint(anchorPoint));

			// get the victims mesh
			_victim_mesh = victim.GetComponent<MeshFilter>().mesh;

			// reset values
			_new_vertices.Clear();

			_leftSide  = new Mesh_Maker();
			_rightSide = new Mesh_Maker();


			bool[] sides = new bool[3];
			int[] indices;
			int   p1,p2,p3;

			// go throught the submeshes
			for(int sub=0; sub<_victim_mesh.subMeshCount; sub++){

				indices = _victim_mesh.GetTriangles(sub);

				for(int i=0; i<indices.Length; i+=3){

					p1 = indices[i];
					p2 = indices[i+1];
					p3 = indices[i+2];

					sides[0] = _blade.GetSide(_victim_mesh.vertices[p1]);
					sides[1] = _blade.GetSide(_victim_mesh.vertices[p2]);
					sides[2] = _blade.GetSide(_victim_mesh.vertices[p3]);


					// whole triangle
					if(sides[0] == sides[1] && sides[0] == sides[2]){

						if(sides[0]){ // left side

							_leftSide.AddTriangle(
								new Vector3[]{ _victim_mesh.vertices[p1], _victim_mesh.vertices[p2], _victim_mesh.vertices[p3] },
								new Vector3[]{ _victim_mesh.normals[p1],  _victim_mesh.normals[p2],  _victim_mesh.normals[p3] },
								new Vector2[]{ _victim_mesh.uv[p1],       _victim_mesh.uv[p2],       _victim_mesh.uv[p3] },
								new Vector4[]{ _victim_mesh.tangents[p1], _victim_mesh.tangents[p2], _victim_mesh.tangents[p3] },
								sub);
						}else{

							_rightSide.AddTriangle(
								new Vector3[]{ _victim_mesh.vertices[p1], _victim_mesh.vertices[p2], _victim_mesh.vertices[p3] },
								new Vector3[]{ _victim_mesh.normals[p1],  _victim_mesh.normals[p2],  _victim_mesh.normals[p3] },
								new Vector2[]{ _victim_mesh.uv[p1],       _victim_mesh.uv[p2],       _victim_mesh.uv[p3] },
								new Vector4[]{ _victim_mesh.tangents[p1], _victim_mesh.tangents[p2], _victim_mesh.tangents[p3] },
								sub);
						}

					}else{ // cut the triangle
						
						Cut_this_Face(
							new Vector3[]{ _victim_mesh.vertices[p1], _victim_mesh.vertices[p2], _victim_mesh.vertices[p3] },
							new Vector3[]{ _victim_mesh.normals[p1],  _victim_mesh.normals[p2],  _victim_mesh.normals[p3] },
							new Vector2[]{ _victim_mesh.uv[p1],       _victim_mesh.uv[p2],       _victim_mesh.uv[p3] },
							new Vector4[]{ _victim_mesh.tangents[p1], _victim_mesh.tangents[p2], _victim_mesh.tangents[p3] },
							sub);
					}
				}
			}

			// The capping Material will be at the end
			Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;
			if(mats[mats.Length-1].name != capMaterial.name){
				Material[] newMats = new Material[mats.Length+1];
				mats.CopyTo(newMats, 0);
				newMats[mats.Length] = capMaterial;
				mats = newMats;
			}
			_capMatSub = mats.Length-1; // for later use

			// cap the opennings
			Capping();


			// Left Mesh
			Mesh left_HalfMesh = _leftSide.GetMesh();
			left_HalfMesh.name =  "Split Mesh Left";

			// Right Mesh
			Mesh right_HalfMesh = _rightSide.GetMesh();
			right_HalfMesh.name = "Split Mesh Right";

			// assign the game objects

			victim.name = "left side";
			victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;

			GameObject leftSideObj = victim;

			GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
			rightSideObj.transform.position = victim.transform.position;
			rightSideObj.transform.rotation = victim.transform.rotation;
			rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;
		
			if(victim.transform.parent != null){
				rightSideObj.transform.parent = victim.transform.parent;
			}

			rightSideObj.transform.localScale = victim.transform.localScale;


			// assign mats
			leftSideObj.GetComponent<MeshRenderer>().materials = mats;
			rightSideObj.GetComponent<MeshRenderer>().materials = mats;

			return new GameObject[]{ leftSideObj, rightSideObj };

		}

		/// <summary>
		///  I have no idea how I made this work
		/// </summary>
		private static void Cut_this_Face(
			Vector3[] vertices,
			Vector3[] normals,
			Vector2[] uvs,
			Vector4[] tangents,
			int       submesh){

			bool[] sides = new bool[3];
			sides[0] = _blade.GetSide(vertices[0]); // true = left
			sides[1] = _blade.GetSide(vertices[1]);
			sides[2] = _blade.GetSide(vertices[2]);


			Vector3[] leftPoints = new Vector3[2];
			Vector3[] leftNormals = new Vector3[2];
			Vector2[] leftUvs = new Vector2[2];
			Vector4[] leftTangents = new Vector4[2];
			Vector3[] rightPoints = new Vector3[2];
			Vector3[] rightNormals = new Vector3[2];
			Vector2[] rightUvs = new Vector2[2];
			Vector4[] rightTangents = new Vector4[2];

			bool didset_left = false;
			bool didset_right = false;

			for(int i=0; i<3; i++){

				if(sides[i]){
					if(!didset_left){
						didset_left = true;

						leftPoints[0]   = vertices[i];
						leftPoints[1]   = leftPoints[0];
						leftUvs[0]     = uvs[i];
						leftUvs[1]     = leftUvs[0];
						leftNormals[0] = normals[i];
						leftNormals[1] = leftNormals[0];
						leftTangents[0] = tangents[i];
						leftTangents[1] = leftTangents[0];

					}else{

						leftPoints[1]   = vertices[i];
						leftUvs[1]      = uvs[i];
						leftNormals[1]  = normals[i];
						leftTangents[1] = tangents[i];

					}
				}else{
					if(!didset_right){
						didset_right = true;

						rightPoints[0]   = vertices[i];
						rightPoints[1]   = rightPoints[0];
						rightUvs[0]     = uvs[i];
						rightUvs[1]     = rightUvs[0];
						rightNormals[0] = normals[i];
						rightNormals[1] = rightNormals[0];
						rightTangents[0] = tangents[i];
						rightTangents[1] = rightTangents[0];

					}else{

						rightPoints[1]  = vertices[i];
						rightUvs[1]     = uvs[i];
						rightNormals[1] = normals[i];
						rightTangents[1] = tangents[i];

					}
				}
			}


			float normalizedDistance = 0.0f;
			float distance = 0;
			_blade.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

			normalizedDistance =  distance/(rightPoints[0] - leftPoints[0]).magnitude;
			Vector3 newVertex1 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
			Vector2 newUv1     = Vector2.Lerp(leftUvs[0], rightUvs[0], normalizedDistance);
			Vector3 newNormal1 = Vector3.Lerp(leftNormals[0] , rightNormals[0], normalizedDistance);
			Vector4 newTangent1 = Vector3.Lerp(leftTangents[0], rightTangents[0], normalizedDistance);		

			_new_vertices.Add(newVertex1);

			_blade.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

			normalizedDistance =  distance/(rightPoints[1] - leftPoints[1]).magnitude;
			Vector3 newVertex2 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
			Vector2 newUv2     = Vector2.Lerp(leftUvs[1], rightUvs[1], normalizedDistance);
			Vector3 newNormal2 = Vector3.Lerp(leftNormals[1] , rightNormals[1], normalizedDistance);
			Vector4 newTangent2 = Vector3.Lerp(leftTangents[1], rightTangents[1], normalizedDistance);		


			_new_vertices.Add(newVertex2);


			Vector3[] final_verts; 
			Vector3[] final_norms;
			Vector2[] final_uvs;
			Vector4[] final_tangents;

			// first triangle

			final_verts = new Vector3[]{leftPoints[0], newVertex1, newVertex2};
			final_norms = new Vector3[]{leftNormals[0], newNormal1, newNormal2};
			final_uvs   = new Vector2[]{leftUvs[0], newUv1, newUv2};
			final_tangents = new Vector4[]{ leftTangents[0], newTangent1, newTangent2};

			if(final_verts[0] != final_verts[1] && final_verts[0] != final_verts[2]){

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_leftSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents, submesh);
			}

			// second triangle

			final_verts = new Vector3[]{leftPoints[0], leftPoints[1], newVertex2};
			final_norms = new Vector3[]{leftNormals[0], leftNormals[1], newNormal2};
			final_uvs   = new Vector2[]{leftUvs[0], leftUvs[1], newUv2};
			final_tangents = new Vector4[]{ leftTangents[0], leftTangents[1], newTangent2};

			if(final_verts[0] != final_verts[1] && final_verts[0] != final_verts[2]){

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_leftSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents, submesh);
			}

			// third triangle

			final_verts = new Vector3[]{rightPoints[0], newVertex1, newVertex2};
			final_norms = new Vector3[]{rightNormals[0], newNormal1, newNormal2};
			final_uvs   = new Vector2[]{rightUvs[0], newUv1, newUv2};
			final_tangents = new Vector4[]{ rightTangents[0], newTangent1, newTangent2};

			if(final_verts[0] != final_verts[1] && final_verts[0] != final_verts[2]){

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_rightSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents, submesh);
			}

			// fourth triangle

			final_verts = new Vector3[]{rightPoints[0], rightPoints[1], newVertex2};
			final_norms = new Vector3[]{rightNormals[0], rightNormals[1], newNormal2};
			final_uvs   = new Vector2[]{rightUvs[0], rightUvs[1], newUv2};
			final_tangents = new Vector4[]{ rightTangents[0], rightTangents[1], newTangent2};

			if(final_verts[0] != final_verts[1] && final_verts[0] != final_verts[2]){

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_rightSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents, submesh);
			}

		}

		private static void FlipFace(
			Vector3[] verts,
			Vector3[] norms,
			Vector2[] uvs, 
			Vector4[] tangents)
		{

			Vector3 temp = verts[2];
			verts[2] = verts[0];
			verts[0] = temp;

			temp = norms[2];
			norms[2] = norms[0];
			norms[0] = temp;

			Vector2 temp2 = uvs[2];
			uvs[2] = uvs[0];
			uvs[0] = temp2;

			Vector4 temp3 = tangents[2];
			tangents[2] = tangents[0];
			tangents[0] = temp3;

		}
			
		private static List<Vector3> capVertTracker = new List<Vector3>();
		private static List<Vector3> capVertpolygon = new List<Vector3>();

		static void Capping(){

			capVertTracker.Clear();

			for(int i=0; i<_new_vertices.Count; i++)
				if(!capVertTracker.Contains(_new_vertices[i]))
				{
					capVertpolygon.Clear();
					capVertpolygon.Add(_new_vertices[i]);
					capVertpolygon.Add(_new_vertices[i+1]);

					capVertTracker.Add(_new_vertices[i]);
					capVertTracker.Add(_new_vertices[i+1]);


					bool isDone = false;
					while(!isDone){
						isDone = true;

						for(int k=0; k<_new_vertices.Count; k+=2){ // go through the pairs

							if(_new_vertices[k] == capVertpolygon[capVertpolygon.Count-1] && !capVertTracker.Contains(_new_vertices[k+1])){ // if so add the other

								isDone = false;
								capVertpolygon.Add(_new_vertices[k+1]);
								capVertTracker.Add(_new_vertices[k+1]);

							}else if(_new_vertices[k+1] == capVertpolygon[capVertpolygon.Count-1] && !capVertTracker.Contains(_new_vertices[k])){// if so add the other

								isDone = false;
								capVertpolygon.Add(_new_vertices[k]);
								capVertTracker.Add(_new_vertices[k]);
							}
						}
					}

					FillCap(capVertpolygon);

				}
			
		}

		static void FillCap(List<Vector3> vertices){


			// center of the cap
			Vector3 center = Vector3.zero;
			foreach(Vector3 point in vertices)
				center += point;

			center = center/vertices.Count;

			// you need an axis based on the cap
			Vector3 upward = Vector3.zero;
			// 90 degree turn
			upward.x = _blade.normal.y;
			upward.y = -_blade.normal.x;
			upward.z = _blade.normal.z;
			Vector3 left = Vector3.Cross(_blade.normal, upward);

			Vector3 displacement = Vector3.zero;
			Vector2 newUV1 = Vector2.zero;
			Vector2 newUV2 = Vector2.zero;

			for(int i=0; i<vertices.Count; i++){

				displacement = vertices[i] - center;
				newUV1 = Vector3.zero;
				newUV1.x = 0.5f + Vector3.Dot(displacement, left);
				newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
				//newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

				displacement = vertices[(i+1) % vertices.Count] - center;
				newUV2 = Vector3.zero;
				newUV2.x = 0.5f + Vector3.Dot(displacement, left);
				newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
				//newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

				Vector3[] final_verts = new Vector3[]{vertices[i], vertices[(i+1) % vertices.Count], center};
				Vector3[] final_norms = new Vector3[]{-_blade.normal, -_blade.normal, -_blade.normal};
				Vector2[] final_uvs   = new Vector2[]{newUV1, newUV2, new Vector2(0.5f, 0.5f)};
				Vector4[] final_tangents = new Vector4[]{ Vector4.zero, Vector4.zero, Vector4.zero};

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_leftSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents,
					_capMatSub);


				final_norms = new Vector3[]{_blade.normal, _blade.normal, _blade.normal};

				if(Vector3.Dot(Vector3.Cross(final_verts[1] - final_verts[0], final_verts[2] - final_verts[0]), final_norms[0]) < 0){
					FlipFace(final_verts, final_norms, final_uvs, final_tangents);
				}

				_rightSide.AddTriangle(final_verts, final_norms, final_uvs, final_tangents,
					_capMatSub);
				

			}
				

		}
	
	}
}
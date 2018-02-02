
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

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using BLINDED_AM_ME._ScriptHelper;
#endif

namespace BLINDED_AM_ME._2D{

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class Spline2D : MonoBehaviour{

#if UNITY_EDITOR

		[Range(0.1f, 5.0f)]
		public float segmentLength = 1.0f;
		[Range(0.01f, 5.0f)]
		public float segmentHeight = 1.0f;
		[Range(-1.0f, 1.0f)]
		public float offset = 0.0f;

		[Range(0.01f, 1.0f)]
		public float colliderHeight = 1f;
		[Range(-1.0f, 1.0f)]
		public float colliderOffset = 0f;

		private Transform  _helpTransform1;
		private Transform  _helpTransform2;

		// Mesh Values
		private List<Vector3>   _vertices  = new List<Vector3>();
		private List<Vector3>   _normals   = new List<Vector3>();
		private List<Vector2>   _uvs       = new List<Vector2>();
		private List<int>       _triangles = new List<int>();
		private List<List<int>> _subIndices = new List<List<int>>();

		// collider points
		private List<Vector2>   _colliderTopPoints = new List<Vector2>();
		private List<Vector2>   _colliderBottomPoints = new List<Vector2>();


		// Path values
		public bool 		_isSmooth  = false;
		public bool         _isCircuit = false;
		private Transform[] _children;
		private Path _path = new Path();
		public float TotalDistance;

		public enum ColliderType{

			Edge,
			Polygon

		};

		public ColliderType colliderType = ColliderType.Edge;

		private void Reset(){

			Transform obj = null;

			if(transform.childCount == 0){

				// make the children
				obj = new GameObject("point1").transform;
				obj.SetParent(transform);
				obj.localPosition = Vector3.left;
				obj.gameObject.AddComponent<SelectableSphereGizmo>();
				obj.gameObject.isStatic = true; 

				obj = new GameObject("point2").transform;
				obj.SetParent(transform);
				obj.localPosition = Vector3.right;
				obj.gameObject.AddComponent<SelectableSphereGizmo>();
				obj.gameObject.isStatic = true;
			}

			ShapeIt();

		}

		private void OnDrawGizmos()
		{
			DrawGizmos(false);
		}
		private void OnDrawGizmosSelected()
		{
			DrawGizmos(true);
		}

		private void DrawGizmos(bool selected)
		{
			
			if(!Application.isPlaying && Selection.activeTransform != null){
				
				if(Selection.activeTransform == this.transform || Selection.activeTransform.parent == this.transform)
					ShapeIt();
			}

		}
			
		private void ShapeIt(){

			if(_helpTransform1 == null)
				_helpTransform1 = new GameObject("_helpTransform1").transform;
			if(_helpTransform2 == null)	
				_helpTransform2 = new GameObject("_helpTransform2").transform;

			// because it messes it up
			Quaternion oldRotation = transform.rotation;
			transform.rotation = Quaternion.identity;

			SetPath();
			ResetLists();
			Craft(); // make segments
			Apply(); // apply values

			transform.rotation = oldRotation;

			if(_helpTransform1 != null)
				DestroyImmediate(_helpTransform1.gameObject);
			if(_helpTransform2 != null)
				DestroyImmediate(_helpTransform2.gameObject);

		}

		public void SetPath(){
			
			_children = new Transform[transform.childCount];
			Vector3[] points = new Vector3[_children.Length];
			Vector3[] ups    = new Vector3[_children.Length];

			for(int i=0; i< transform.childCount; i++){
				_children[i] = transform.GetChild(i);
				_children[i].gameObject.name = "point " + i;

				points[i] = _children[i].localPosition;
				ups[i] = transform.InverseTransformDirection(_children[i].up);
			}

			if (transform.childCount > 1){
				_path.SetPoints(points, ups, _isCircuit);
			}

			TotalDistance = _path.TotalDistance;
		}
			
		public void Craft(){


			Path_Point pointA = _path.GetPathPoint(0.0f, _isSmooth);
			Path_Point pointB = pointA;


			for(float dist=0.0f; dist<_path.TotalDistance; dist+=segmentLength){

				pointB = _path.GetPathPoint(Mathf.Clamp(dist + segmentLength,0,_path.TotalDistance), _isSmooth);

				_helpTransform1.rotation = Quaternion.LookRotation(pointA.forward, pointA.up);
				_helpTransform1.position = transform.TransformPoint(pointA.point);

				_helpTransform2.rotation = Quaternion.LookRotation(pointB.forward, pointB.up);
				_helpTransform2.position = transform.TransformPoint(pointB.point);

				Add_Segment();

				pointA = pointB;
			}

		}
			
		private static Vector3[] _fourCorners = new Vector3[4];
		public void Add_Segment(){

			// square clockwise (bottom Left - bottom right)
			_fourCorners[0] = new Vector3(0,-segmentHeight * 0.5f,0);               // 0,0
			_fourCorners[1] = new Vector3(0, segmentHeight * 0.5f,0);               // 0,1
			_fourCorners[2] = new Vector3(0, segmentHeight * 0.5f, segmentLength);  // 1,1
			_fourCorners[3] = new Vector3(0, -segmentHeight * 0.5f, segmentLength); // 1,0

			_fourCorners[0].y += offset;
			_fourCorners[1].y += offset;
			_fourCorners[2].y += offset;
			_fourCorners[3].y += offset;

			// adjust to the new locations
			float lerpValue = 0.0f;
			Vector3 pointA, pointB;
			for(int i=0; i<4; i++){

				lerpValue = Math_Functions.Value_from_another_Scope(_fourCorners[i].z, 0, segmentLength, 0.0f, 1.0f);
				_fourCorners[i].z = 0.0f;

				pointA = _helpTransform1.TransformPoint(_fourCorners[i]); // to world
				pointB = _helpTransform2.TransformPoint(_fourCorners[i]);

				_fourCorners[i] = transform.InverseTransformPoint(Vector3.Lerp(pointA, pointB,lerpValue)); // to local
			}

			// adding 2 triangles
			AddTriangle(new Vector3[]{
				_fourCorners[0],
				_fourCorners[1],
				_fourCorners[2]

			}, new Vector2[]{
				new Vector2(0f,0f),
				new Vector2(0f,1f),
				new Vector2(1f,1f)
			});

			AddTriangle(new Vector3[]{
				_fourCorners[3],
				_fourCorners[0],
				_fourCorners[2]

			}, new Vector2[]{
				new Vector2(1f,0f),
				new Vector2(0f,0f),
				new Vector2(1f,1f)
			});

			// collider points

			// square clockwise (bottom Left - bottom right)
			_fourCorners[0] = new Vector3(0,-segmentHeight * 0.5f * colliderHeight,0);               // 0,0
			_fourCorners[1] = new Vector3(0, segmentHeight * 0.5f * colliderHeight,0);               // 0,1
			_fourCorners[2] = new Vector3(0, segmentHeight * 0.5f * colliderHeight, segmentLength);  // 1,1
			_fourCorners[3] = new Vector3(0, -segmentHeight * 0.5f * colliderHeight, segmentLength); // 1,0

			_fourCorners[0].y += colliderOffset * segmentHeight;
			_fourCorners[1].y += colliderOffset * segmentHeight;
			_fourCorners[2].y += colliderOffset * segmentHeight;
			_fourCorners[3].y += colliderOffset * segmentHeight;

			// adjust to the new locations
			for(int i=0; i<4; i++){

				lerpValue = Math_Functions.Value_from_another_Scope(_fourCorners[i].z, 0, segmentLength, 0.0f, 1.0f);
				_fourCorners[i].z = 0.0f;

				pointA = _helpTransform1.TransformPoint(_fourCorners[i]); // to world
				pointB = _helpTransform2.TransformPoint(_fourCorners[i]);

				_fourCorners[i] = transform.InverseTransformPoint(Vector3.Lerp(pointA, pointB,lerpValue)); // to local
			}

			// collider points
			if(!_colliderBottomPoints.Contains(_fourCorners[0]))
				_colliderBottomPoints.Add(_fourCorners[0]);
			if(!_colliderTopPoints.Contains(_fourCorners[1]))
				_colliderTopPoints.Add(_fourCorners[1]);
			if(!_colliderTopPoints.Contains(_fourCorners[2]))
				_colliderTopPoints.Add(_fourCorners[2]);
			if(!_colliderBottomPoints.Contains(_fourCorners[3]))
				_colliderBottomPoints.Add(_fourCorners[3]);

		}

		public void AddTriangle(Vector3[] verts, Vector2[] uvs){


			Vector3 calculated_normal = Vector3.Cross((verts[1] - verts[0]).normalized, (verts[2] - verts[0]).normalized);


			if(Vector3.Dot(calculated_normal, Vector3.back) < 0){

				Vector3 temp  = verts[2];
				verts[2] = verts[0];
				verts[0] = temp;

				Vector2 temp2 = uvs[2];
				uvs[2] = uvs[0];
				uvs[0] = temp2;
			}


			for(int i=0; i<3; i++){
				
				// is this new?
				int index = -1;

				for(int iterator=0; iterator<_vertices.Count; iterator++){
					if(_vertices[iterator] == verts[i] &&
						_uvs[iterator] == uvs[i]){
						index = iterator;
						break;
					}
				}

				// it is
				if(index == -1){
					_subIndices[0].Add(_vertices.Count);
					_triangles.Add(_vertices.Count);
					_vertices.Add(verts[i]);
					_normals.Add(Vector3.back);
					_uvs.Add(uvs[i]);

				}else{ // it is not
					_subIndices[0].Add(index);
					_triangles.Add(index);
				}

			}
		}


		public void ResetLists(){

			_vertices.Clear();
			_normals.Clear();
			_uvs.Clear();
			_triangles.Clear();
			_subIndices.Clear();
			_subIndices.Add(new List<int>());

			_colliderTopPoints.Clear();
			_colliderBottomPoints.Clear();

		}


		public void Apply(){

			Mesh _mesh      = new Mesh();
			_mesh.name =  "mesh"+Selection.activeInstanceID;
			_mesh.vertices  = _vertices.ToArray();
			_mesh.triangles = _triangles.ToArray();
			_mesh.normals   = _normals.ToArray();
			_mesh.uv        = _uvs.ToArray();
		
			_mesh.subMeshCount = 1;
			_mesh.SetIndices(_subIndices[0].ToArray(), MeshTopology.Triangles, 0);	

			// for light mapping
		//	UnityEditor.Unwrapping.GenerateSecondaryUVSet(trail);

			GetComponent<MeshFilter>().mesh = _mesh;

			if(_isCircuit){
				_colliderTopPoints.Add(_colliderTopPoints[0]);
				_colliderBottomPoints.Add(_colliderBottomPoints[0]);
			}
				
			if(colliderType == ColliderType.Edge){

				if(GetComponent<PolygonCollider2D>())
					DestroyImmediate(GetComponent<PolygonCollider2D>());

				EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
				if(edge == null)
					edge = gameObject.AddComponent<EdgeCollider2D>();

				edge.points = _colliderTopPoints.ToArray();


			}else{

				if(GetComponent<EdgeCollider2D>())
					DestroyImmediate(GetComponent<EdgeCollider2D>());

				PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
				if(poly == null)
					poly = gameObject.AddComponent<PolygonCollider2D>();
				

				for(int i=_colliderBottomPoints.Count-1; i>=0; i--)
					_colliderTopPoints.Add(_colliderBottomPoints[i]);
				
				poly.points = _colliderTopPoints.ToArray();
			}



		}


#endif
	}
}
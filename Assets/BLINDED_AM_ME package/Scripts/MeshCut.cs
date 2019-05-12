
//    MIT License
//    
//    Copyright (c) 2017 Dustin Whirle
//    
//    My Yrefube stuff: https://www.yrefube.com/playlist?list=PL-sp8pM7xzbVls1NovXqwgfBQiwhTA_Ya
//    
//    Permission is hereby granted, free of charge, to any person obtaining a copy
//    of this software and associated documentation files (the "Software"), to deal
//    in the Software withref restriction, including withref limitation the rights
//    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//    copies of the Software, and to permit persons to whom the Software is
//    furnished to do so, subject to the following conditions:
//    
//    The above copyright notice and this permission notice shall be included in all
//    copies or substantial portions of the Software.
//    
//    THE SOFTWARE IS PROVIDED "AS IS", WITHref WARRANTY OF ANY KIND, EXPRESS OR
//    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//    ref OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//    SOFTWARE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BLINDED_AM_ME{

	public class MeshCut{
        
		private static Plane _blade;
		private static Mesh  _victim_mesh;

        // Caching
        private static Mesh_Maker _leftSide = new Mesh_Maker();
		private static Mesh_Maker _rightSide = new Mesh_Maker();
        private static Mesh_Maker.Triangle _cacheTriangle = new Mesh_Maker.Triangle( new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3] );
        private static List<Vector3>       _newVertices = new List<Vector3>();
        private static bool[]              _isLeftSideCache = new bool[3];
        private static int                 _capMatSub = 1;
       
        /// <summary>
        /// Yeah
        /// </summary>
        /// <param name="victim">self discribed</param>
        /// <param name="anchorPoint">blade world position</param>
        /// <param name="normalDirection">blade right direction</param>
        /// <param name="capMaterial">Meat</param>
        /// <returns></returns>
        public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial){
           
			// set the blade relative to victim
			_blade = new Plane(victim.transform.InverseTransformDirection(-normalDirection),
				victim.transform.InverseTransformPoint(anchorPoint));

			// get the victims mesh
			_victim_mesh = victim.GetComponent<MeshFilter>().mesh;

            // two new meshes
            _leftSide.Clear();
            _rightSide.Clear();
            _newVertices.Clear();

            
			int   index_1, index_2, index_3;

            var mesh_vertices = _victim_mesh.vertices;
            var mesh_normals  = _victim_mesh.normals;
            var mesh_uvs      = _victim_mesh.uv;
            var mesh_tangents = _victim_mesh.tangents;

			// go through the submeshes
			for (int submeshIterator=0; submeshIterator<_victim_mesh.subMeshCount; submeshIterator++){

                // Triangles
				var indices = _victim_mesh.GetTriangles(submeshIterator);
                
				for(int i=0; i<indices.Length; i+=3){

					index_1 = indices[i];
					index_2 = indices[i+1];
					index_3 = indices[i+2];

                    // verts
                    _cacheTriangle.vertices[0] = mesh_vertices[index_1];
                    _cacheTriangle.vertices[1] = mesh_vertices[index_2];
                    _cacheTriangle.vertices[2] = mesh_vertices[index_3];

                    // normals
                    _cacheTriangle.normals[0] = mesh_normals[index_1];
                    _cacheTriangle.normals[1] = mesh_normals[index_2];
                    _cacheTriangle.normals[2] = mesh_normals[index_3];

                    // uvs
                    _cacheTriangle.uvs[0] = mesh_uvs[index_1];
                    _cacheTriangle.uvs[1] = mesh_uvs[index_2];
                    _cacheTriangle.uvs[2] = mesh_uvs[index_3];

                    // tangents
                    _cacheTriangle.tangents[0] = mesh_tangents[index_1];
                    _cacheTriangle.tangents[1] = mesh_tangents[index_2];
                    _cacheTriangle.tangents[2] = mesh_tangents[index_3];


                    // which side are the vertices on
                    _isLeftSideCache[0] = _blade.GetSide(mesh_vertices[index_1]);
					_isLeftSideCache[1] = _blade.GetSide(mesh_vertices[index_2]);
					_isLeftSideCache[2] = _blade.GetSide(mesh_vertices[index_3]);
                    

					// whole triangle
					if(_isLeftSideCache[0] == _isLeftSideCache[1] && _isLeftSideCache[0] == _isLeftSideCache[2]){

						if(_isLeftSideCache[0]) // left side
							_leftSide.AddTriangle( _cacheTriangle, submeshIterator);
						else // right side
							_rightSide.AddTriangle(_cacheTriangle, submeshIterator);

					}else{ // cut the triangle
						
						Cut_this_Face(ref _cacheTriangle, submeshIterator);
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
			Cap_the_Cut();


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

        #region Cutting
        // Caching
        private static Mesh_Maker.Triangle _cacheLeftTriangle  = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static Mesh_Maker.Triangle _cacheRightTriangle = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        private static Mesh_Maker.Triangle _cacheNewTriangle  = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
        // Functions
        private static void Cut_this_Face(ref Mesh_Maker.Triangle triangle, int submesh)
        {

            _isLeftSideCache[0] = _blade.GetSide(triangle.vertices[0]); // true = left
            _isLeftSideCache[1] = _blade.GetSide(triangle.vertices[1]);
            _isLeftSideCache[2] = _blade.GetSide(triangle.vertices[2]);


            int leftCount = 0;
            int rightCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (_isLeftSideCache[i])
                { // left

                    _cacheLeftTriangle.vertices[leftCount] = triangle.vertices[i];
                    _cacheLeftTriangle.uvs[leftCount] = triangle.uvs[i];
                    _cacheLeftTriangle.normals[leftCount] = triangle.normals[i];
                    _cacheLeftTriangle.tangents[leftCount] = triangle.tangents[i];
                    
                    leftCount++;
                }
                else
                { // right

                    _cacheRightTriangle.vertices[rightCount] = triangle.vertices[i];
                    _cacheRightTriangle.uvs[rightCount] = triangle.uvs[i];
                    _cacheRightTriangle.normals[rightCount] = triangle.normals[i];
                    _cacheRightTriangle.tangents[rightCount] = triangle.tangents[i];

                    rightCount++;
                }
            }

            // find the new triangles X 3
            // first the new vertices

            // this will give me a triangle with the solo point as first
            if (leftCount == 1)
            {
                _cacheTriangle.vertices[0] = _cacheLeftTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheLeftTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheLeftTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheLeftTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheRightTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheRightTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheRightTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheRightTriangle.tangents[0];
                                                   
                _cacheTriangle.vertices[2] = _cacheRightTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheRightTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheRightTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheRightTriangle.tangents[1];
            }
            else // rightCount == 1
            {
                _cacheTriangle.vertices[0] = _cacheRightTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheRightTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheRightTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheRightTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheLeftTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheLeftTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheLeftTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheLeftTriangle.tangents[0];
                                                   
                _cacheTriangle.vertices[2] = _cacheLeftTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheLeftTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheLeftTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheLeftTriangle.tangents[1];
            }

            // now to find the intersection points between the solo point and the others
            float distance = 0;
            float normalizedDistance = 0.0f;
            Vector3 edgeVector = Vector3.zero; // contains edge length and direction

            edgeVector = _cacheTriangle.vertices[1] - _cacheTriangle.vertices[0];
            _blade.Raycast(new Ray(_cacheTriangle.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _cacheNewTriangle.vertices[0] = Vector3.Lerp(_cacheTriangle.vertices[0], _cacheTriangle.vertices[1], normalizedDistance);
            _cacheNewTriangle.uvs[0]      = Vector2.Lerp(_cacheTriangle.uvs[0],      _cacheTriangle.uvs[1],      normalizedDistance);
            _cacheNewTriangle.normals[0]  = Vector3.Lerp(_cacheTriangle.normals[0],  _cacheTriangle.normals[1],  normalizedDistance);
            _cacheNewTriangle.tangents[0] = Vector4.Lerp(_cacheTriangle.tangents[0], _cacheTriangle.tangents[1], normalizedDistance);

            edgeVector = _cacheTriangle.vertices[2] - _cacheTriangle.vertices[0];
            _blade.Raycast(new Ray(_cacheTriangle.vertices[0], edgeVector.normalized), out distance);

            normalizedDistance = distance / edgeVector.magnitude;
            _cacheNewTriangle.vertices[1] = Vector3.Lerp(_cacheTriangle.vertices[0], _cacheTriangle.vertices[2], normalizedDistance);
            _cacheNewTriangle.uvs[1]      = Vector2.Lerp(_cacheTriangle.uvs[0],      _cacheTriangle.uvs[2],      normalizedDistance);
            _cacheNewTriangle.normals[1]  = Vector3.Lerp(_cacheTriangle.normals[0],  _cacheTriangle.normals[2],  normalizedDistance);
            _cacheNewTriangle.tangents[1] = Vector4.Lerp(_cacheTriangle.tangents[0], _cacheTriangle.tangents[2], normalizedDistance);

            if (_cacheNewTriangle.vertices[0] != _cacheNewTriangle.vertices[1])
            {
                //tracking newly created points
                _newVertices.Add(_cacheNewTriangle.vertices[0]);
                _newVertices.Add(_cacheNewTriangle.vertices[1]);
            }
            // make the new triangles
            // one side will get 1 the other will get 2

            if (leftCount == 1)
            {
                // first one on the left
                _cacheTriangle.vertices[0] = _cacheLeftTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheLeftTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheLeftTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheLeftTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheNewTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheNewTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheNewTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheNewTriangle.tangents[0];
                                                   
                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];
                
                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _leftSide.AddTriangle(_cacheTriangle, submesh);


                // other two on the right
                _cacheTriangle.vertices[0] = _cacheRightTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheRightTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheRightTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheRightTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheNewTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheNewTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheNewTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheNewTriangle.tangents[0];

                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];
                
                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _rightSide.AddTriangle(_cacheTriangle, submesh);

                // third
                _cacheTriangle.vertices[0] = _cacheRightTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheRightTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheRightTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheRightTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheRightTriangle.vertices[1];
                _cacheTriangle.uvs[1]      = _cacheRightTriangle.uvs[1];
                _cacheTriangle.normals[1]  = _cacheRightTriangle.normals[1];
                _cacheTriangle.tangents[1] = _cacheRightTriangle.tangents[1];

                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _rightSide.AddTriangle(_cacheTriangle, submesh);
            }
            else
            {
                // first one on the right
                _cacheTriangle.vertices[0] = _cacheRightTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheRightTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheRightTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheRightTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheNewTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheNewTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheNewTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheNewTriangle.tangents[0];

                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _rightSide.AddTriangle(_cacheTriangle, submesh);


                // other two on the left
                _cacheTriangle.vertices[0] = _cacheLeftTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheLeftTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheLeftTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheLeftTriangle.tangents[0];

                _cacheTriangle.vertices[1] = _cacheNewTriangle.vertices[0];
                _cacheTriangle.uvs[1]      = _cacheNewTriangle.uvs[0];
                _cacheTriangle.normals[1]  = _cacheNewTriangle.normals[0];
                _cacheTriangle.tangents[1] = _cacheNewTriangle.tangents[0];

                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _leftSide.AddTriangle(_cacheTriangle, submesh);

                // third
                _cacheTriangle.vertices[0] = _cacheLeftTriangle.vertices[0];
                _cacheTriangle.uvs[0]      = _cacheLeftTriangle.uvs[0];
                _cacheTriangle.normals[0]  = _cacheLeftTriangle.normals[0];
                _cacheTriangle.tangents[0] = _cacheLeftTriangle.tangents[0];
                                                   
                _cacheTriangle.vertices[1] = _cacheLeftTriangle.vertices[1];
                _cacheTriangle.uvs[1]      = _cacheLeftTriangle.uvs[1];
                _cacheTriangle.normals[1]  = _cacheLeftTriangle.normals[1];
                _cacheTriangle.tangents[1] = _cacheLeftTriangle.tangents[1];

                _cacheTriangle.vertices[2] = _cacheNewTriangle.vertices[1];
                _cacheTriangle.uvs[2]      = _cacheNewTriangle.uvs[1];
                _cacheTriangle.normals[2]  = _cacheNewTriangle.normals[1];
                _cacheTriangle.tangents[2] = _cacheNewTriangle.tangents[1];

                // check if it is facing the right way
                NormalCheck(ref _cacheTriangle);

                // add it
                _leftSide.AddTriangle(_cacheTriangle, submesh);
            }
            
        }
        #endregion

        #region Capping
        // Caching
        private static List<int> _capUsedIndices = new List<int>();
		private static List<int> _capPolygonIndices = new List<int>();
        // Functions
		static void Cap_the_Cut(){

            _capUsedIndices.Clear();
            _capPolygonIndices.Clear();

            // find the needed polygons
            // the cut faces added new vertices by 2 each time to make an edge
            // if two edges contain the same Vector3 point, they are connected
            for (int i = 0; i < _newVertices.Count; i+=2)
            {
                // check the edge
                if (!_capUsedIndices.Contains(i)) // if it has one, it has this edge
                {
                    //new polygon started with this edge
                    _capPolygonIndices.Clear();
                    _capPolygonIndices.Add(i);
                    _capPolygonIndices.Add(i + 1);

                    _capUsedIndices.Add(i);
                    _capUsedIndices.Add(i + 1);

                    Vector3 connectionPointLeft  = _newVertices[i];
                    Vector3 connectionPointRight = _newVertices[i + 1];
                    bool isDone = false;

                    // look for more edges
                    while (!isDone)
                    {
                        isDone = true;

                        // loop through edges
                        for (int index = 0; index < _newVertices.Count; index += 2)
                        {   // if it has one, it has this edge
                            if (!_capUsedIndices.Contains(index)) 
                            {
                                Vector3 nextPoint1 = _newVertices[index];
                                Vector3 nextPoint2 = _newVertices[index + 1];

                                // check for next point in the chain
                                if (connectionPointLeft == nextPoint1 ||
                                    connectionPointLeft == nextPoint2 ||
                                    connectionPointRight == nextPoint1 ||
                                    connectionPointRight == nextPoint2)
                                {
                                    _capUsedIndices.Add(index);
                                    _capUsedIndices.Add(index + 1);

                                    // add the other
                                    if (connectionPointLeft == nextPoint1)
                                    {
                                        _capPolygonIndices.Insert(0, index + 1);
                                        connectionPointLeft = _newVertices[index + 1];
                                    }
                                    else if (connectionPointLeft == nextPoint2)
                                    {
                                        _capPolygonIndices.Insert(0, index);
                                        connectionPointLeft = _newVertices[index];
                                    }
                                    else if (connectionPointRight == nextPoint1)
                                    {
                                        _capPolygonIndices.Add(index + 1);
                                        connectionPointRight = _newVertices[index + 1];
                                    }
                                    else if (connectionPointRight == nextPoint2)
                                    {
                                        _capPolygonIndices.Add(index);
                                        connectionPointRight = _newVertices[index];
                                    }
                                    
                                    isDone = false;
                                }
                            }
                        }
                    }// while isDone = False
                    
                    // check if the link is closed
                    // first == last
                     if (_newVertices[_capPolygonIndices[0]] == _newVertices[_capPolygonIndices[_capPolygonIndices.Count - 1]])
                        _capPolygonIndices[_capPolygonIndices.Count - 1] = _capPolygonIndices[0];
                    else
                        _capPolygonIndices.Add(_capPolygonIndices[0]);

                    FillCap(_capPolygonIndices);
                }
            }
		}
		static void FillCap(List<int> indices){
            
            // center of the cap
            Vector3 center = Vector3.zero;
			foreach(var index in indices)
				center += _newVertices[index];

			center = center/indices.Count;

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

			for(int i=0; i<indices.Count-1; i++){

				displacement = _newVertices[indices[i]] - center;
				newUV1 = Vector3.zero;
				newUV1.x = 0.5f + Vector3.Dot(displacement, left);
				newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
				//newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

				displacement = _newVertices[indices[i+1]] - center;
				newUV2 = Vector3.zero;
				newUV2.x = 0.5f + Vector3.Dot(displacement, left);
				newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
                //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);



                _cacheNewTriangle.vertices[0] = _newVertices[indices[i]];
                _cacheNewTriangle.uvs[0]      = newUV1;
                _cacheNewTriangle.normals[0]  = -_blade.normal;
                _cacheNewTriangle.tangents[0] = Vector4.zero;
                
                _cacheNewTriangle.vertices[1] = _newVertices[indices[i + 1]];
                _cacheNewTriangle.uvs[1]      = newUV2;
                _cacheNewTriangle.normals[1]  = -_blade.normal;
                _cacheNewTriangle.tangents[1] = Vector4.zero;
                
                _cacheNewTriangle.vertices[2] = center;
                _cacheNewTriangle.uvs[2]      = new Vector2(0.5f, 0.5f);
                _cacheNewTriangle.normals[2]  = -_blade.normal;
                _cacheNewTriangle.tangents[2] = Vector4.zero;

            
                NormalCheck(ref _cacheNewTriangle);

                _leftSide.AddTriangle(_cacheNewTriangle, _capMatSub);

                _cacheNewTriangle.normals[0] = _blade.normal;
                _cacheNewTriangle.normals[1] = _blade.normal;
                _cacheNewTriangle.normals[2] = _blade.normal;

                NormalCheck(ref _cacheNewTriangle);

                _rightSide.AddTriangle(_cacheNewTriangle, _capMatSub);

            }

		}
        #endregion

        #region Misc.
        private static void NormalCheck(ref Mesh_Maker.Triangle triangle)
        {
            Vector3 crossProduct = Vector3.Cross(triangle.vertices[1] - triangle.vertices[0], triangle.vertices[2] - triangle.vertices[0]);
            Vector3 averageNormal = (triangle.normals[0] + triangle.normals[1] + triangle.normals[2]) / 3.0f;
            float dotProduct = Vector3.Dot(averageNormal, crossProduct);
            if (dotProduct < 0)
            {
                Vector3 temp = triangle.vertices[2];
                triangle.vertices[2] = triangle.vertices[0];
                triangle.vertices[0] = temp;

                temp = triangle.normals[2];
                triangle.normals[2] = triangle.normals[0];
                triangle.normals[0] = temp;

                Vector2 temp2 = triangle.uvs[2];
                triangle.uvs[2] = triangle.uvs[0];
                triangle.uvs[0] = temp2;

                Vector4 temp3 = triangle.tangents[2];
                triangle.tangents[2] = triangle.tangents[0];
                triangle.tangents[0] = temp3;
            }

        }
        #endregion
    }
}
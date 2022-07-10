using BLINDED_AM_ME.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME.Extensions
{
    public static class MeshExtensions
    {
        /// <summary> Creates a MeshMaker from Unity Mesh </summary>
        /// <remarks> Must be done on the main thread </remarks>
        public static MeshMaker ToMeshMaker(this Mesh mesh)
        {
            // get mesh info // creates new lists
            var meshMaker = new MeshMaker
            {
                Vertices = mesh.vertices.ToList(),
                Uvs = mesh.uv.ToList(),
                Normals = mesh.normals.ToList(),
                Tangents = mesh.tangents.ToList(),
                Submeshes = new List<List<int>>(mesh.subMeshCount)
            };

            for (int submesh_i = 0; submesh_i < mesh.subMeshCount; submesh_i++)
                meshMaker.Submeshes.Add(mesh.GetTriangles(submesh_i).ToList());

            return meshMaker;
        }

        #region Cutting
        /// <summary> Call on UI Thread </summary>
        public static Tuple<Mesh, Mesh> Cut(this Mesh mesh, Plane blade, int capSubMeshIndex = 0, CancellationToken cancellationToken = default)
        {
            // this has to be done on UI thread
            var maker = mesh.ToMeshMaker();

            // Starts a new thread
            var pieces = Task.Run(() => CutTaskAsync(maker, blade, capSubMeshIndex, cancellationToken)).Result;
            // Comes back with result

            // Left Mesh
            Mesh left_HalfMesh = pieces.Item1.GetMesh();
            left_HalfMesh.name = "Split Mesh Left";

            // Right Mesh
            Mesh right_HalfMesh = pieces.Item2.GetMesh();
            right_HalfMesh.name = "Split Mesh Right";

            return new Tuple<Mesh, Mesh>(left_HalfMesh, right_HalfMesh);
        }
        
        /// <summary> Call and Wait on UI Thread </summary>
        public static IEnumerator CutCoroutine(this Mesh mesh, Plane blade, Action<Tuple<Mesh, Mesh>> callback = null, int capSubMeshIndex = 0, CancellationToken cancellationToken = default)
        {
            // this has to be done on UI thread
            var maker = mesh.ToMeshMaker();

            // Starts a new thread
            var task = Task.Run(() => CutTaskAsync(maker, blade, capSubMeshIndex, cancellationToken));
            yield return task.WaitForTask((pieces) =>
            {
                // comes back to UI thread

                // Left Mesh
                Mesh left_HalfMesh = pieces.Item1.GetMesh();
                left_HalfMesh.name = "Split Mesh Left";

                // Right Mesh
                Mesh right_HalfMesh = pieces.Item2.GetMesh();
                right_HalfMesh.name = "Split Mesh Right";

                callback?.Invoke(new Tuple<Mesh, Mesh>(left_HalfMesh, right_HalfMesh));
            });
        }

        /// <summary> Done in the background </summary>
        private static async Task<Tuple<MeshMaker, MeshMaker>> CutTaskAsync(MeshMaker sourceMesh, Plane blade, int capSubMeshIndex = 0, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // iterators
            int index_1, index_2, index_3;
            
            // triangle values
            // when a triangle is cut, two new points are made
            // each has a vertex, uv, normal, and tangent 
            Vector3 A_Vertex,  B_Vertex,  C_Vertex;
            Vector2 A_Uv,      B_Uv,      C_Uv;
            Vector3 A_Normal,  B_Normal,  C_Normal;
            Vector4 A_Tangent, B_Tangent, C_Tangent;

            #region Cutting
            
            bool side_1,  side_2, side_3;
            int rotation, rotations, index_temp;
            
            // intersection values
            Vector3 AB, AC;
            float t1, t2;
            Ray ray = new Ray();

            // new points will be added to the end
            int createdIndexA, createdIndexB;
            int createdCount = 0;

            var leftSubmeshes = new List<List<int>>(sourceMesh.Submeshes.Count);
            var rightSubmeshes = new List<List<int>>(sourceMesh.Submeshes.Count);
            for (int submesh_i = 0; submesh_i < sourceMesh.Submeshes.Count; submesh_i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var submesh = sourceMesh.Submeshes[submesh_i];
                var leftSubmesh = new List<int>(submesh.Count);
                var rightSubmesh = new List<int>(submesh.Count);

                // matching number of submeshes
                leftSubmeshes.Add(leftSubmesh);
                rightSubmeshes.Add(rightSubmesh);

                // Triangles come in threes
                for (int i = 0; i < submesh.Count; i += 3)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // indices 
                    index_1 = submesh[i];
                    index_2 = submesh[i + 1];
                    index_3 = submesh[i + 2];

                    // which side are the vertices on
                    side_1 = blade.GetSide(sourceMesh.Vertices[index_1]);
                    side_2 = blade.GetSide(sourceMesh.Vertices[index_2]);
                    side_3 = blade.GetSide(sourceMesh.Vertices[index_3]);

                    // whole triangle
                    if (side_1 == side_2
                     && side_2 == side_3)
                    {
                        // Right side
                        if (side_1)
                        {
                            rightSubmesh.Add(index_1);
                            rightSubmesh.Add(index_2);
                            rightSubmesh.Add(index_3);
                        }
                        else // Left side
                        {
                            leftSubmesh.Add(index_1);
                            leftSubmesh.Add(index_2);
                            leftSubmesh.Add(index_3);
                        }
                    }
                    else // cut Triangle
                    {
                        // make the solo vert the first
                        rotations = 0; // 1st
                        if (side_2 != side_1
                         && side_2 != side_3)
                            rotations = 2; // 2nd
                        else if (side_3 != side_1
                              && side_3 != side_2)
                            rotations = 1; // 3rd

                        // rotation to keep order
                        // for rendering
                        for (rotation = 0; rotation < rotations; rotation++)
                        {
                            index_temp = index_3;
                            index_3 = index_2;
                            index_2 = index_1;
                            index_1 = index_temp;
                        }

                        // Get Values
                        A_Vertex = sourceMesh.Vertices[index_1];
                        A_Uv = sourceMesh.Uvs[index_1];
                        A_Normal = sourceMesh.Normals[index_1];
                        A_Tangent = sourceMesh.Tangents[index_1];

                        B_Vertex = sourceMesh.Vertices[index_2];
                        B_Uv = sourceMesh.Uvs[index_2];
                        B_Normal = sourceMesh.Normals[index_2];
                        B_Tangent = sourceMesh.Tangents[index_2];

                        C_Vertex = sourceMesh.Vertices[index_3];
                        C_Uv = sourceMesh.Uvs[index_3];
                        C_Normal = sourceMesh.Normals[index_3];
                        C_Tangent = sourceMesh.Tangents[index_3];

                        // now to find the intersection values
                        // between the solo point and the others

                        // direction and length
                        AB = B_Vertex - A_Vertex;
                        ray.origin = A_Vertex;
                        ray.direction = AB.normalized; // direction
                        blade.Raycast(ray, out float distance_1);

                        AC = C_Vertex - A_Vertex;
                        ray.direction = AC.normalized;
                        blade.Raycast(ray, out float distance_2);

                        t1 = distance_1 / AB.magnitude; // length
                        t2 = distance_2 / AC.magnitude;

                        // created A values
                        createdIndexA = sourceMesh.AddValues(
                            Vector3.Lerp(A_Vertex, B_Vertex, t1),
                            Vector2.Lerp(A_Uv, B_Uv, t1),
                            Vector3.Lerp(A_Normal, B_Normal, t1),
                            Vector4.Lerp(A_Tangent, B_Tangent, t1));

                        // created B values
                        createdIndexB = sourceMesh.AddValues(
                            Vector3.Lerp(A_Vertex, C_Vertex, t2),
                            Vector2.Lerp(A_Uv, C_Uv, t2),
                            Vector3.Lerp(A_Normal, C_Normal, t2),
                            Vector4.Lerp(A_Tangent, C_Tangent, t2));

                        createdCount += 2;

                        // save triangles
                        if (blade.GetSide(A_Vertex))
                        {
                            rightSubmeshes[submesh_i].Add(index_1);
                            rightSubmeshes[submesh_i].Add(createdIndexA);
                            rightSubmeshes[submesh_i].Add(createdIndexB);

                            leftSubmeshes[submesh_i].Add(createdIndexA);
                            leftSubmeshes[submesh_i].Add(index_2);
                            leftSubmeshes[submesh_i].Add(index_3);

                            leftSubmeshes[submesh_i].Add(index_3);
                            leftSubmeshes[submesh_i].Add(createdIndexB);
                            leftSubmeshes[submesh_i].Add(createdIndexA);
                        }
                        else
                        {
                            leftSubmeshes[submesh_i].Add(index_1);
                            leftSubmeshes[submesh_i].Add(createdIndexA);
                            leftSubmeshes[submesh_i].Add(createdIndexB);

                            rightSubmeshes[submesh_i].Add(createdIndexA);
                            rightSubmeshes[submesh_i].Add(index_2);
                            rightSubmeshes[submesh_i].Add(index_3);

                            rightSubmeshes[submesh_i].Add(index_3);
                            rightSubmeshes[submesh_i].Add(createdIndexB);
                            rightSubmeshes[submesh_i].Add(createdIndexA);
                        }
                    }
                }
            }
            #endregion

            #region Capping

            // new points were added to the end
            var createdIndex = sourceMesh.Vertices.Count - createdCount;
            var createdEdges = sourceMesh.Vertices.GetRange(createdIndex, createdCount);
            var availableEdges = new List<Vector3>(createdEdges);// the countdown

            var capPolygons = new List<List<Vector3>>();
            while (availableEdges.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // new polygon with 1st link
                var polygon = new List<Vector3>(availableEdges.Count)
                {
                   availableEdges[0], availableEdges[1]
                };

                // don't count twice
                availableEdges.RemoveRange(0, 2);

                // for Comparing
                var polygonHead = polygon.First();
                var polygonTail = polygon.Last();

                // Connect the links in the chain
                bool isDone = false;
                while (!isDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    isDone = true;

                    // find the next link // edges come in pairs
                    for (var i = 0; i < availableEdges.Count; i+=2)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        A_Vertex = availableEdges[i];
                        B_Vertex = availableEdges[i + 1];

                        if (A_Vertex == polygonTail // Tails
                         || B_Vertex == polygonTail)
                        {
                            polygonTail = A_Vertex == polygonTail ? B_Vertex : A_Vertex;
                            polygon.Add(polygonTail);

                            availableEdges.RemoveRange(i, 2);

                            isDone = false;
                            break;
                        }
                        else if (A_Vertex == polygonHead // Heads
                              || B_Vertex == polygonHead)
                        {
                            polygonHead = A_Vertex == polygonHead ? B_Vertex : A_Vertex;
                            polygon.Insert(0, polygonHead);

                            availableEdges.RemoveRange(i, 2);

                            isDone = false;
                            break;
                        }
                    }

                    // polygon complete
                    if (polygonHead == polygonTail)
                    {
                        polygon.RemoveAt(0);// Head or Tail
                        isDone = true;
                        break;
                    }
                }

                // needs to be at least one triangle
                if (polygon.Count() >= 3)
                    capPolygons.Add(polygon);
            }

            // Get Triangles
            MeshMaker capMesh = new MeshMaker();
            foreach (var polygon in capPolygons)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // center of the cap or atleast the average
                var center = Vector3.zero;
                foreach (var vertex in polygon)
                    center += vertex;

                center /= polygon.Count;

                // 3D axis
                var up = (polygon.First() - center).normalized;
                var right = Vector3.Cross(up, blade.normal);

                // Uvs = 2D points
                var polygon2D = new Vector2[polygon.Count];

                // Get Values
                var uvOffset = Vector3.zero;
                var indexOffset = capMesh.Vertices.Count;
                for (int i = 0; i < polygon.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // 3D to 2D
                    uvOffset = polygon[i] - center;
                    polygon2D[i].x = 0.5f + Vector3.Dot(uvOffset, right);
                    polygon2D[i].y = 0.5f + Vector3.Dot(uvOffset, up);
                }

                // Get Triangles of 2D Polygon
                var triangles = await MathExtensions.Geometry.TriangulateNonConvexPolygonAsync(polygon2D, cancellationToken);

                // add triangles
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    // triangles indices are of the polygon
                    index_1 = triangles[i];
                    index_2 = triangles[i + 1];
                    index_3 = triangles[i + 2];

                    // check which side it will be facing
                    if (!TriangleNormalCheck(polygon[index_1],
                                             polygon[index_2],
                                             polygon[index_3],
                                             blade.normal))
                    {
                        // The last shall be first and the first last.
                        var x = index_1;
                        index_1 = index_3;
                        index_3 = x;
                    }

                    // generate tangents
                    GetTangentsForTriangle(
                        polygon[index_1],   polygon[index_2],   polygon[index_3],
                        polygon2D[index_1], polygon2D[index_2], polygon2D[index_3],
                        blade.normal, blade.normal, blade.normal,
                        out Vector4 tangent_1, out Vector4 tangent_2, out Vector4 tangent_3);
                    
                    // add
                    capMesh.AddTriangle(
                        capMesh.AddValues(
                            polygon[index_1],
                            polygon2D[index_1],
                            blade.normal, 
                            tangent_1), 
                        capMesh.AddValues(
                            polygon[index_2],
                            polygon2D[index_2],
                            blade.normal, 
                            tangent_2),
                        capMesh.AddValues(
                            polygon[index_3],
                            polygon2D[index_3],
                            blade.normal, 
                            tangent_3),
                        capSubMeshIndex);

                }
            }
            #endregion

            // get non cap values
            var leftSide = sourceMesh.ExtractSubmeshes(leftSubmeshes, cancellationToken);
            var rightSide = sourceMesh.ExtractSubmeshes(rightSubmeshes, cancellationToken);

            // add cap to the left
            await leftSide.AddAsync(capMesh, cancellationToken);

            // flip over cap
            await capMesh.FlipTrianglesAsync(cancellationToken);

            // add cap to the right
            await rightSide.AddAsync(capMesh, cancellationToken);

            return new Tuple<MeshMaker, MeshMaker>(leftSide, rightSide);
        }
        #endregion  
        
        /// <summary> Determines if the triangle order will match the normal direction </summary>
        /// <remarks> When adding a triangle to a mesh, the order will determine which side is visible.
        /// Unless using a shader with backface culling off </remarks>
        /// <returns> Vector3.Dot(normal, Vector3.Cross(v1 - v2, v3 - v2)) > 0 </returns>
        public static bool TriangleNormalCheck(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
        {
            return Vector3.Dot(normal, Vector3.Cross(v2 - v1, v3 - v1)) > 0;
        }

        public static void GetTangentsForTriangle(
            Vector3 vertice_1, Vector3 vertice_2, Vector3 vertice_3,
            Vector2 uv_1, Vector2 uv_2, Vector2 uv_3,
            Vector3 normal_1, Vector3 normal_2, Vector3 normal_3,
            out Vector4 tangent_1, out Vector4 tangent_2, out Vector4 tangent_3)
        {

            Vector3 v_AB = vertice_2 - vertice_1;
            Vector3 v_AC = vertice_3 - vertice_1;

            Vector2 uv_AB = uv_2 - uv_1;
            Vector2 uv_AC = uv_3 - uv_1;

            Vector3 tan1 = new(
                uv_AC.y * v_AB.x - uv_AB.y * v_AC.x,
                uv_AC.y * v_AB.y - uv_AB.y * v_AC.y,
                uv_AC.y * v_AB.z - uv_AB.y * v_AC.z);
            Vector3 tan2 = new(
                uv_AB.x * v_AC.x - uv_AC.x * v_AB.x,
                uv_AB.x * v_AC.y - uv_AC.x * v_AB.y,
                uv_AB.x * v_AC.z - uv_AC.x * v_AB.z);

            float fraction = 1.0f / MathExtensions.Geometry.Cross(uv_AB, uv_AC);
            tan1 *= fraction;
            tan2 *= fraction;

            var tangent = tan1;
            Vector3.OrthoNormalize(ref normal_1, ref tangent);

            tangent_1 = new Vector4(
            tangent.x,
            tangent.y,
            tangent.z,
            Vector3.Dot(tan2, Vector3.Cross(normal_1, tangent)) < 0 ? -1 : 1);

            tangent = tan1;
            Vector3.OrthoNormalize(ref normal_2, ref tangent);

            tangent_2 = new Vector4(
            tangent.x,
            tangent.y,
            tangent.z,
            Vector3.Dot(tan2, Vector3.Cross(normal_2, tangent)) < 0 ? -1 : 1);

            tangent = tan1;
            Vector3.OrthoNormalize(ref normal_3, ref tangent);

            tangent_3 = new Vector4(
            tangent.x,
            tangent.y,
            tangent.z,
            Vector3.Dot(tan2, Vector3.Cross(normal_3, tangent)) < 0 ? -1 : 1);
        }

    }

}

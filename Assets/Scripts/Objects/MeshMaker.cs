using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME.Objects
{
    public class MeshMaker
    {
        // Mesh Values
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Vector2> Uvs { get; set; } = new List<Vector2>();
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
        public List<Vector4> Tangents { get; set; } = new List<Vector4>();
        public List<List<int>> Submeshes { get; set; } = new List<List<int>>();

        

        /// <returns> Index </returns>
        public int AddValues(Vector3 vertex, Vector2 uv, Vector3 normal, Vector4 tangent)
        {
            int index = Vertices.Count;

            Vertices.Add(vertex);
            Uvs.Add(uv);
            Normals.Add(normal);
            Tangents.Add(tangent);

            return index;
        }

        /// <summary> Adds a new indices to submesh </summary>
        public void AddTriangle(int index1, int index2, int index3, int submesh = 0)
        {
            // add needed
            for (int i = Submeshes.Count; i <= submesh; i++)
                Submeshes.Add(new List<int>());

            Submeshes[submesh].Add(index1);
            Submeshes[submesh].Add(index2);
            Submeshes[submesh].Add(index3);
        }

        /// <summary> Flips direction of all triangles and normals </summary>
        public void FlipTriangles(CancellationToken cancellationToken = default)
        {
            Task.Run(() => FlipTrianglesAsync(cancellationToken), cancellationToken).Wait();
        }
        public Task FlipTrianglesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                // flip triangles
                int temp;
                foreach (var submesh in Submeshes)
                {
                    for (int i = 0; i < submesh.Count; i += 3)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // The last shall be first and the first last.
                        temp = submesh[i + 2];
                        submesh[i + 2] = submesh[i];
                        submesh[i] = temp;
                    }
                }

                for (int i = 0; i < Normals.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Normals[i] = Normals[i] * -1.0f;
                }
            });
        }
        
        /// <summary> Cleans up Double Vertices </summary>
        /// <returns> number of doubles removed </returns>
        public int RemoveDoubles(CancellationToken cancellationToken = default)
        {
            return Task.Run(() => RemoveDoublesAsync(cancellationToken), cancellationToken).Result;
        }
        public Task<int> RemoveDoublesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                int dubCount = 0;

                var negIterator = 0; // backward
                for (var iterator = 0; iterator < Vertices.Count; iterator++)
                {
                    // look backwards for a match
                    for (negIterator = iterator - 1; negIterator >= 0; negIterator--)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Is Duplicate
                        if (Vertices[iterator] == Vertices[negIterator]
                         && Uvs[iterator] == Uvs[negIterator]
                         && Normals[iterator] == Normals[negIterator]
                         && Tangents[iterator] == Tangents[negIterator])
                        {
                            dubCount++;
                            foreach (var submesh in Submeshes)
                                for (int index = 0; index < submesh.Count; index++)
                                    if (submesh[index] > iterator) // knock it down
                                        submesh[index]--;
                                    else if (submesh[index] == iterator) // replace
                                        submesh[index] = negIterator;

                            Vertices.RemoveAt(iterator);
                            Uvs.RemoveAt(iterator);
                            Normals.RemoveAt(iterator);
                            Tangents.RemoveAt(iterator);

                            iterator--;
                            break; // there should only be one
                        }
                    }
                }

                return dubCount;
            });
        }

        public MeshMaker ExtractSubmeshes(IEnumerable<IEnumerable<int>> submeshes, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => ExtractSubmeshesAsync(submeshes, cancellationToken), cancellationToken).Result;
        }
        public Task<MeshMaker> ExtractSubmeshesAsync(IEnumerable<IEnumerable<int>> submeshes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.Run(() =>
            {
                var returnMesh = new MeshMaker();

                var indices = new List<int>();
                foreach (var submesh in submeshes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    indices.AddRange(submesh);
                }

                // old, new
                var cypher = new Dictionary<int, int>();

                // get unique values
                var counter = returnMesh.Vertices.Count;
                foreach (var index in indices.Distinct())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    returnMesh.Vertices.Add(Vertices[index]);
                    returnMesh.Uvs.Add(Uvs[index]);
                    returnMesh.Normals.Add(Normals[index]);
                    returnMesh.Tangents.Add(Tangents[index]);

                    // old, new
                    cypher.Add(index, counter);
                    counter++;
                }

                // submeshes
                int submesh_i = -1;
                int triangleCount = 0;
                foreach (var submesh in submeshes)
                {
                    submesh_i++;

                    // triangles
                    triangleCount = submesh.Count();
                    for (int triangle = 0; triangle < triangleCount; triangle += 3)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        returnMesh.AddTriangle(cypher[submesh.ElementAt(triangle)],
                                               cypher[submesh.ElementAt(triangle + 1)],
                                               cypher[submesh.ElementAt(triangle + 2)],
                                               submesh_i);
                    }
                }

                return returnMesh;
            });
        }

        /// <summary> Adds all values and triangles from sourceMesh to this </summary>
        public void Add(MeshMaker sourceMesh, CancellationToken cancellationToken = default)
        {
            Task.Run(() => AddAsync(sourceMesh, cancellationToken), cancellationToken).Wait();
        }
        public Task AddAsync(MeshMaker sourceMesh, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.Run(() =>
            {
                var indices = new List<int>();
                foreach (var submesh in sourceMesh.Submeshes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    indices.AddRange(submesh);
                }

                // offset
                var indexOffset = Vertices.Count;

                // add values
                for (int i = 0; i < sourceMesh.Vertices.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    Vertices.Add(sourceMesh.Vertices[i]);
                    Uvs.Add(sourceMesh.Uvs[i]);
                    Normals.Add(sourceMesh.Normals[i]);
                    Tangents.Add(sourceMesh.Tangents[i]);
                }

                // submesh
                var submesh_i = -1;
                foreach (var submesh in sourceMesh.Submeshes)
                {
                    submesh_i++;

                    // triangles
                    for (int triangle = 0; triangle < submesh.Count; triangle += 3)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        AddTriangle(indexOffset + submesh[triangle],
                                    indexOffset + submesh[triangle + 1],
                                    indexOffset + submesh[triangle + 2],
                                    submesh_i);
                    }
                }
            });
        }

        /// <summary> Creates and returns a new mesh </summary>
        public Mesh GetMesh()
        {
            var shape = new Mesh();
            shape.name = "Generated Mesh "+shape.GetInstanceID();
            shape.SetVertices(Vertices);
            shape.SetNormals(Normals);
            shape.SetUVs(0, Uvs);
            shape.SetUVs(1, Uvs);
            shape.SetTangents(Tangents);
            shape.subMeshCount = Submeshes.Count;

            int submesh_i = 0;
            foreach (var submesh in Submeshes)
            {
                shape.SetTriangles(submesh, submesh_i);
                submesh_i++;
            }
            return shape;
        }
#if UNITY_EDITOR
        /// <summary> Creates and returns a new mesh with generated lightmap uvs (Editor Only) </summary>
        public Mesh GetMesh_GenerateSecondaryUVSet()
        {
            Mesh shape = GetMesh();

            // for light mapping
            UnityEditor.Unwrapping.GenerateSecondaryUVSet(shape);

            return shape;
        }

        /// <summary> Creates and returns a new mesh with generated lightmap uvs (Editor Only) </summary>
        public Mesh GetMesh_GenerateSecondaryUVSet(UnityEditor.UnwrapParam param)
        {
            Mesh shape = GetMesh();

            // for light mapping
            UnityEditor.Unwrapping.GenerateSecondaryUVSet(shape, param);

            return shape;
        }
#endif
    }

}

using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME.Extensions;
using BLINDED_AM_ME.Objects;

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class Spline : Path
	{
		public Mesh segmentMesh;

		public Spline() { }
		
		// UI Thread
		private CancellationTokenSource _previousTaskCancel;
		public void GenerateMesh(CancellationToken cancellationToken = default)
        {
			if (segmentMesh == null)
			{
				Debug.LogError("Missing Source Mesh");
				return;
			}

			_previousTaskCancel?.Cancel();
			_previousTaskCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cancellationToken = _previousTaskCancel.Token;

			StartCoroutine(GenerateMeshCoroutine(cancellationToken));
        }

		// UI Thread
		private IEnumerator GenerateMeshCoroutine(CancellationToken cancellationToken = default)
		{
			var sourceMeshMaker = segmentMesh.ToMeshMaker();
			var worldToLocal = transform.worldToLocalMatrix;
			
			yield return GenerateMeshTaskAsync(sourceMeshMaker, worldToLocal, cancellationToken)
				.WaitForTask((generatedMesh) => 
			{ 

#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					switch (lightmapUnwrapping)
					{
						case LightmapUnwrapping.UseFirstUvSet:
							GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh();
							break;
						case LightmapUnwrapping.DefaultUnwrapParam:
							GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh_GenerateSecondaryUVSet();
							break;
						default:
							GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh();
							break;
					}
				}
				else
				{
					GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh();
				}
#else
			GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh();
#endif
			});
		}

		// Other Thread
		private Task<MeshMaker> GenerateMeshTaskAsync(MeshMaker sourceMesh, Matrix4x4 worldToLocalMatrix, CancellationToken cancellationToken = default)
		{
			// starts on new thread
			return Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();

				var targetMesh = new MeshMaker();

				float segment_length;
				float segment_MinZ = 0.0f;
				float segment_MaxZ = 0.0f;

				// find length along z axis
				foreach (var vertex in sourceMesh.Vertices)
				{
					cancellationToken.ThrowIfCancellationRequested();

					// Min
					if (vertex.z < segment_MinZ)
						segment_MinZ = vertex.z;

					// Max
					if (vertex.z > segment_MaxZ)
						segment_MaxZ = vertex.z;
				}
				segment_length = segment_MaxZ - segment_MinZ;

				Matrix4x4 localToWorldA = GetMatrix(0);  //Matrix4x4.TRS(origin, Quaternion.LookRotation(forward, up), Vector3.one);
				Matrix4x4 localToWorldB = localToWorldA;

				var totalDistance = TotalDistance;
				for (float dist = segment_length; dist < totalDistance+ segment_length; dist += segment_length)
				{
					cancellationToken.ThrowIfCancellationRequested();

					localToWorldB = GetMatrix(Math.Min(dist, totalDistance));

					// go through the values
					var indexOffset = targetMesh.Vertices.Count;
					for (int index = 0; index < sourceMesh.Vertices.Count; index++)
					{
						cancellationToken.ThrowIfCancellationRequested();

						// MultiplyVector
						// This function is similar to MultiplyPoint;
						// but it transforms directions and not positions.
						// When transforming a direction, only the rotation part of the matrix is taken into account.

						var vert = sourceMesh.Vertices[index];

						var t = MathExtensions.ConvertRange(vert.z, segment_MinZ, segment_MaxZ, 0.0f, 1.0f);
						vert.z = 0;

						targetMesh.Vertices.Add(
							worldToLocalMatrix.MultiplyPoint3x4(
								Vector3.Lerp(
									localToWorldA.MultiplyPoint3x4(vert),
									localToWorldB.MultiplyPoint3x4(vert),
									t)));

						targetMesh.Uvs.Add(sourceMesh.Uvs[index]);

						targetMesh.Normals.Add(
							worldToLocalMatrix.MultiplyVector(
								Vector3.Lerp(
									localToWorldA.MultiplyVector(sourceMesh.Normals[index]),
									localToWorldB.MultiplyVector(sourceMesh.Normals[index]),
									t)));

						targetMesh.Tangents.Add(
							worldToLocalMatrix.MultiplyVector(
								Vector3.Lerp(
									localToWorldA.MultiplyVector(sourceMesh.Tangents[index]),
									localToWorldB.MultiplyVector(sourceMesh.Tangents[index]),
									t)));
					}

					// go through the submeshes
					var submesh_i = -1;
					foreach (var submesh in sourceMesh.Submeshes)
					{
						submesh_i++;
						for (int triangle_i = 0; triangle_i < submesh.Count; triangle_i += 3)
						{
							cancellationToken.ThrowIfCancellationRequested();

							targetMesh.AddTriangle(
								indexOffset + submesh[triangle_i],
								indexOffset + submesh[triangle_i + 1],
								indexOffset + submesh[triangle_i + 2],
								submesh_i);
						}
					}

					localToWorldA = localToWorldB;
				}

				return targetMesh;
			});
		}

#if UNITY_EDITOR
		public enum LightmapUnwrapping
		{
			DefaultUnwrapParam,
			UseFirstUvSet
		}
		public LightmapUnwrapping lightmapUnwrapping = LightmapUnwrapping.DefaultUnwrapParam;

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
        }
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
        }
#endif
	}
}
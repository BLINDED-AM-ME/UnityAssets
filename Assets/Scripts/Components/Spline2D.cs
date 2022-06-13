﻿using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME.Extensions;
using System.Linq;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using BLINDED_AM_ME.Objects;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class Spline2D : Path
	{
		[SerializeField]
		[SerializeProperty(nameof(SegmentLength))]
		[Range(0.1f, 5.0f)]
		private float _segmentLength = 1.0f;
		public float SegmentLength
		{
			get => _segmentLength;
			set => SetProperty(ref _segmentLength, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(SegmentHeight))]
		[Range(0.01f, 5.0f)]
		private float _segmentHeight = 1.0f;
		public float SegmentHeight
		{
			get => _segmentHeight;
			set => SetProperty(ref _segmentHeight, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(Offset))]
		[Range(-1.0f, 1.0f)]
		private float _offset = 0.0f;
		public float Offset
		{
			get => _offset;
			set => SetProperty(ref _offset, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(ColliderHeight))]
		[Range(0.01f, 1.0f)]
		private float _colliderHeight = 1f;
		public float ColliderHeight
		{
			get => _colliderHeight;
			set => SetProperty(ref _colliderHeight, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(ColliderOffset))]
		[Range(-1.0f, 1.0f)]
		private float _colliderOffset = 0f;
		public float ColliderOffset
		{
			get => _colliderOffset;
			set => SetProperty(ref _colliderOffset, value);
		}

		public enum ColliderType { Edge, Polygon };
		[SerializeField]
		[SerializeProperty(nameof(Collider))]
		private ColliderType _colliderType = ColliderType.Edge;
		public ColliderType Collider
		{
			get => _colliderType;
			set => SetProperty(ref _colliderType, value);
		}
		 
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
			switch (propertyName)
			{
				case nameof(ColliderHeight):
				case nameof(ColliderOffset):
				case nameof(Collider):
					GenerateCollider(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
					break;

				default:
					GenerateMesh(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
					break;
			}

			base.OnPropertyChanged(propertyName);
        }
		protected override void OnPathChanged()
        {
			base.OnPathChanged();
			GenerateMesh(new CancellationTokenSource(TimeSpan.FromSeconds(10)).Token);
        }
		
		// UI Thread
		private CancellationTokenSource _previousTaskCancel;
		public void GenerateMesh(CancellationToken cancellationToken = default)
        {
			_previousTaskCancel?.Cancel();
			_previousTaskCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cancellationToken = _previousTaskCancel.Token;

			StartCoroutine(GenerateMeshCoroutine(cancellationToken));
        }

		// UI Thread
		private IEnumerator GenerateMeshCoroutine(CancellationToken cancellationToken = default)
		{
			var worldToLocal = transform.worldToLocalMatrix;
			yield return GenerateMeshTaskAsync(worldToLocal, cancellationToken)
				.WaitForTask((generatedMesh) =>
				{
					GetComponent<MeshFilter>().mesh = generatedMesh.GetMesh();
				});

			yield return GenerateColliderCoroutine(cancellationToken);
		}

		// Background Thread
		private Task<MeshMaker> GenerateMeshTaskAsync(Matrix4x4 worldToLocal, CancellationToken cancellationToken = default)
        {
			// starts on new thread
			return Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();

				var targetMesh = new MeshMaker();

				// vertices
				Vector3 vertUp = 0.5f * SegmentHeight * Vector3.up;
				vertUp += Vector3.up * Offset;
				Vector3 vertDown = 0.5f * SegmentHeight * Vector3.down;
				vertDown += Vector3.up * Offset;
				
				var totalDistance = TotalDistance;

				cancellationToken.ThrowIfCancellationRequested();
				Matrix4x4 matrixA = GetMatrix(0);
				Matrix4x4 matrixB;
				int i1, i2, i3, i4;
				for (float dist = SegmentLength; dist < totalDistance + SegmentLength; dist += SegmentLength)
				{
					cancellationToken.ThrowIfCancellationRequested();

					matrixB = GetMatrix(Math.Min(dist, totalDistance));

					// square clockwise (bottom Left - bottom right)
					i1 = targetMesh.AddValues(
							worldToLocal.MultiplyPoint3x4(matrixA.MultiplyPoint3x4(vertDown)),
							Vector2.zero,
							Vector3.right,
							Vector4.zero);
					
					i2 = targetMesh.AddValues(
							worldToLocal.MultiplyPoint3x4(matrixA.MultiplyPoint3x4(vertUp)), 
							new Vector2(0,1),
							Vector3.right,
							Vector4.zero);
					
					i3 = targetMesh.AddValues(
							worldToLocal.MultiplyPoint3x4(matrixB.MultiplyPoint3x4(vertUp)), 
							Vector2.one,
                            Vector3.right,
							Vector4.zero);

					i4 = targetMesh.AddValues(
							worldToLocal.MultiplyPoint3x4(matrixB.MultiplyPoint3x4(vertDown)), 
							new Vector2(1,0),
							Vector3.right,
							Vector4.zero);

					targetMesh.AddTriangle(i1, i2, i3);
					targetMesh.AddTriangle(i3, i4, i1);

					matrixA = matrixB;
				}

				return targetMesh;
			});
		}

		// UI Thread
		private CancellationTokenSource _previousColliderTaskCancel;
		private void GenerateCollider(CancellationToken cancellationToken = default)
		{
			_previousColliderTaskCancel?.Cancel();
			_previousColliderTaskCancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cancellationToken = _previousColliderTaskCancel.Token;

			StartCoroutine(GenerateColliderCoroutine(cancellationToken));
		}

		// UI Thread
		private IEnumerator GenerateColliderCoroutine(CancellationToken cancellationToken = default)
		{
			var worldToLocal = transform.worldToLocalMatrix;
			yield return GenerateColliderTaskAsync(worldToLocal, cancellationToken)
				.WaitForTask((colliderPoints) =>
				{
					if (Collider == ColliderType.Edge)
					{

#if UNITY_EDITOR
						if (GetComponent<PolygonCollider2D>())
							if(Application.isPlaying)
								Destroy(GetComponent<PolygonCollider2D>());
							else
								DestroyImmediate(GetComponent<PolygonCollider2D>());
#else
						if (GetComponent<PolygonCollider2D>())
							Destroy(GetComponent<PolygonCollider2D>());
#endif


						EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
						if (edge == null)
							edge = gameObject.AddComponent<EdgeCollider2D>();

						edge.points = colliderPoints;
					}
					else
					{

#if UNITY_EDITOR
						if (GetComponent<EdgeCollider2D>())
							if (Application.isPlaying)
								Destroy(GetComponent<EdgeCollider2D>());
							else
								DestroyImmediate(GetComponent<EdgeCollider2D>());
#else
						if (GetComponent<EdgeCollider2D>())
							Destroy(GetComponent<EdgeCollider2D>());
#endif

						PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
						if (poly == null)
							poly = gameObject.AddComponent<PolygonCollider2D>();

						poly.points = colliderPoints;
					}
				});
		}

		// Background Thread
		private Task<Vector2[]> GenerateColliderTaskAsync(Matrix4x4 worldToLocal, CancellationToken cancellationToken = default)
        {
			// starts on new thread
			return Task.Run(() =>
			{
				// collider points
				Vector2 colliderUp = 0.5f * ColliderHeight * Vector2.up;
				colliderUp += Vector2.up * ColliderOffset;
				Vector2 colliderDown = 0.5f * ColliderHeight * Vector2.down;
				colliderDown += Vector2.up * ColliderOffset;

				var totalDistance = TotalDistance;

				var estNum = (int)(totalDistance / SegmentLength) + 2;
				var topColliderPoints = new List<Vector2>(estNum * 4);
				var bottomColliderPoints = new List<Vector2>(estNum * 4);

				Matrix4x4 matrix;
				for (float dist = 0; dist < totalDistance + SegmentLength; dist += SegmentLength)
				{
					cancellationToken.ThrowIfCancellationRequested();

					matrix = GetMatrix(Math.Min(dist, totalDistance));
					topColliderPoints.Add(worldToLocal.MultiplyPoint3x4(matrix.MultiplyPoint3x4(colliderUp)));
					bottomColliderPoints.Add(worldToLocal.MultiplyPoint3x4(matrix.MultiplyPoint3x4(colliderDown)));
				}

				if (Collider == ColliderType.Edge)
				{
					return topColliderPoints.ToArray();
				}
				else
				{
					bottomColliderPoints.Reverse();
					topColliderPoints.AddRange(bottomColliderPoints);

					return topColliderPoints.ToArray();
				}
			});
		}

	}
}
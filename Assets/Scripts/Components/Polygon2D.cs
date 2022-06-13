using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using BLINDED_AM_ME.Extensions;
using UnityEngine.Events;
using BLINDED_AM_ME.Objects;
using BLINDED_AM_ME.Components;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(PolygonCollider2D))]
	public class Polygon2D : NotifyTransform
	{
		// Clockwise
		private NotifyList<Vector3> _polygon = new NotifyList<Vector3>()
		{
			Vector2.zero,
			Vector2.up,
			Vector2.one,
			Vector2.right
		};

		public Polygon2D()
		{
			_polygon.CollectionChanged += (sender, args) => GenerateMesh();
		}

        protected override void Start()
		{
			base.Start();
			UpdatePolygon();
		}

		protected override void OnChildAdded(Transform child)
		{
			if (!child.TryGetComponent(out NotifyTransform notifyTransform))
				notifyTransform = child.gameObject.AddComponent<NotifyTransform>();

			notifyTransform.PositionChanged += Child_PositionChanged;

			base.OnChildAdded(child);
			UpdatePolygon();
		}
		protected override void OnChildRemoved(Transform child)
		{
			if (child.TryGetComponent(out NotifyTransform notifyTransform))
				notifyTransform.PositionChanged -= Child_PositionChanged;

			base.OnChildRemoved(child);
			UpdatePolygon();
		}

		private void Child_PositionChanged(object sender, ItemEventArgs<Vector3> args)
		{
			UpdatePolygon();
		}

		private void UpdatePolygon()
		{
			// Remove
			if (_polygon.Count > transform.childCount)
			{
				var count = _polygon.Count - transform.childCount;
				var index = _polygon.Count - count;
				_polygon.RemoveRange(index, count);
			}

			int i = -1;
			foreach (Transform child in transform)
            {
				i++;
				child.gameObject.name = "Vert " + i;

				if (i < _polygon.Count)
					_polygon[i] = child.localPosition;
				else
					_polygon.Add(child.localPosition);
            }
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

					PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
					poly.points = _polygon.Select(p => (Vector2)p).ToArray();
				});

		}

		// Background Thread
		private async Task<MeshMaker> GenerateMeshTaskAsync(Matrix4x4 worldToLocal, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var targetMesh = new MeshMaker();

			foreach (var point in _polygon)
				targetMesh.AddValues(point, point, Vector3.back, Vector4.zero);

			var triangles = await MathExtensions.Geometry.TriangulatePolygonAsync(
				_polygon.Select(p => (Vector2)p),
				cancellationToken);

			targetMesh.Submeshes.Add(triangles.ToList());

			return targetMesh;
		}

	}
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using BLINDED_AM_ME.Extensions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME._2D
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(PolygonCollider2D))]
	public class Polygon2D : MonoBehaviour 
	{

		// Clockwise
		private NotifyList<Vector3> _points = new NotifyList<Vector3>() 
		{ 
			Vector2.zero, 
			Vector2.up, 
			Vector2.one, 
			Vector2.right 
		};

		public Polygon2D()
        {
            _points.CollectionChanged += Points_CollectionChanged;
        }
		
		protected virtual void Start()
		{
			if (transform.childCount == 0)
			{
				for (int i = 0; i < _points.Count; i++)
				{
					Transform obj = new GameObject($"vert {i}").transform;
					obj.SetParent(transform);
					obj.localPosition = _points[i];

					if (gameObject.isStatic)
						obj.gameObject.isStatic = true;
				}

				GenerateMesh();
			}
			else
			{
				UpdatePolygon();
			}
		}

		private void Points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
			GenerateMesh();
        }

		public void UpdatePolygon()
		{
			Transform[] children = new Transform[transform.childCount];
			Vector3[] points = new Vector3[children.Length];

			for (int i = 0; i < transform.childCount; i++)
			{
				children[i] = transform.GetChild(i);
				children[i].gameObject.name = "Point " + i;

				points[i] = children[i].localPosition;
			}

			// Add
			if (_points.Count < points.Length)
			{
				var count = points.Length - _points.Count;
				var index = points.Length - count;

				var values = new Vector3[count];
				for (int i = 0; i < count; i++)
					values[i] = points[index + i];

				_points.AddRange(values);
			}

			// Remove
			if (_points.Count > points.Length)
			{
				var count = _points.Count - points.Length;
				var index = _points.Count - count;
				_points.RemoveRange(index, count);
			}

			// Update
			for (var i = 0; i < points.Length; i++)
				_points[i] = points[i];
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
			UpdatePolygon();

			var worldToLocal = transform.worldToLocalMatrix;
			yield return GenerateMeshTaskAsync(worldToLocal, cancellationToken)
				.WaitForTask((generatedMesh) =>
				{
					GetComponent<MeshFilter>().mesh = generatedMesh.ToMesh();

					PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
					poly.points = _points.Select(p => (Vector2)p).ToArray();
				});

		}

		// Background Thread
		private async Task<MeshMaker> GenerateMeshTaskAsync(Matrix4x4 worldToLocal, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var targetMesh = new MeshMaker();

			foreach (var point in _points)
				targetMesh.AddValues(point, point, Vector3.back, Vector4.zero);

			var triangles = await MathExtensions.Geometry.TriangulatePolygonAsync(
				_points.Select(p => (Vector2)p),
				cancellationToken);

			targetMesh.Submeshes.Add(triangles.ToList());

			return targetMesh;
		}

#if UNITY_EDITOR

		protected virtual void OnDrawGizmos()
		{
			var childrenCount = transform.childCount;
			for (int i = 0; i < childrenCount; i++)
			{
				if (!UnityEditor.Selection.transforms.Contains(transform.GetChild(i)))
				{
					OnDrawGizmosSelected();
					return;
				}
			}

			DrawGizmos(false);
		}

		protected virtual void OnDrawGizmosSelected()
		{
			UpdatePolygon();
			DrawGizmos(true);
		}

		private void DrawGizmos(bool isSelected)
        {

        }

#endif

	}
}
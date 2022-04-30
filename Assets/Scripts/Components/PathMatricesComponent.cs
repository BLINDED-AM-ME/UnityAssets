using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME
{
	[ExecuteInEditMode]
	public class PathMatricesComponent : MonoBehaviour
	{
		public bool isSmooth = true;
		public bool isCircuit = false;

		public PathMatrices Path { get; } = new PathMatrices();
        public float TotalDistance => Path.GetTotalDistance(isCircuit);

		public PathMatricesComponent()
        {
            Path.CollectionChanged += Path_CollectionChanged;
		}

        protected virtual void Start()
		{
			if (transform.childCount == 0)
			{
				Transform obj = null;

				// make the children
				obj = new GameObject("point1").transform;
				obj.SetParent(transform);
				obj.localPosition = Vector3.zero;

				if (gameObject.isStatic)
					obj.gameObject.isStatic = true;

				obj = new GameObject("point2").transform;
				obj.SetParent(transform);
				obj.localPosition = Vector3.forward;

				if (gameObject.isStatic)
					obj.gameObject.isStatic = true;
			}
			
			UpdatePath();
		}

		private void Path_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
			OnPathChanged();
        }

		protected virtual void OnPathChanged()
		{
			
		}

		public void UpdatePath()
		{
			Transform[] children = new Transform[transform.childCount];
			Matrix4x4[] localToWorldPoints = new Matrix4x4[children.Length];

			for (int i = 0; i < transform.childCount; i++)
			{
				children[i] = transform.GetChild(i);
				children[i].gameObject.name = "point " + i;

				localToWorldPoints[i] = children[i].localToWorldMatrix;
			}

			// Add
			if (Path.Count < localToWorldPoints.Length)
			{
				var count = localToWorldPoints.Length - Path.Count;
				var index = localToWorldPoints.Length - count;

				var values = new Matrix4x4[count];
				for (int i = 0; i < count; i++)
					values[i] = localToWorldPoints[index + i];

				Path.AddRange(values);
			}

			// Remove
			if (Path.Count > localToWorldPoints.Length)
			{
				var count = Path.Count - localToWorldPoints.Length;
				var index = Path.Count - count;
				Path.RemoveRange(index, count);
			}

			// Update
			for (var i = 0; i < localToWorldPoints.Length; i++)
				Path[i] = localToWorldPoints[i];
		}

		public Vector3 GetPoint(float distance)
		{
			return Path.GetPoint(distance, isSmooth, isCircuit);
		}

		/// <summary> Following the Path </summary>
		/// <remarks> MultiplyVector(Vector3.forward) will point along the path</remarks>
		/// <returns> a Matrix dependent on the up axis of nearby Matrices </returns>
		public Matrix4x4 GetMatrixFollowing(float distance)
        {
			return Path.GetMatrixFollowing(distance, isSmooth, isCircuit);
        }

		/// <returns> A fully blended Matrix </returns>
		public Matrix4x4 GetMatrixRaw(float distance)
        {
			return Path.GetMatrixRaw(distance, isSmooth, isCircuit);
        }

		/// <summary> Following the Path </summary>
		/// <remarks> forward will point along the path </remarks>
		public void GetAxesFollowing(float distance, out Vector3 origin, out Vector3 right, out Vector3 up, out Vector3 forward)
		{
			Path.GetAxesFollowing(distance, isSmooth, isCircuit, out origin, out right, out up, out forward);
		}

		public void GetAxesRaw(float distance, out Vector3 origin, out Vector3 right, out Vector3 up, out Vector3 forward)
		{
			Path.GetAxesRaw(distance, isSmooth, isCircuit, out origin, out right, out up, out forward);
		}

#if UNITY_EDITOR

		[Range(0.01f, 1.0f)]
		public float gizmoLineSize = 0.5f;

		[Range(0.0f, 1.0f)]
		public float gizmoPathPoint = 0.0f;

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
			UpdatePath();
			DrawGizmos(true);
		}

		private void DrawGizmos(bool isSelected)
		{
			if (Path.Count < 2)
				return;

			if (!isSmooth)
			{
				Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);

				for (var i = 0; i < Path.Count - 1; i++)
					Gizmos.DrawLine(Path[i].MultiplyPoint3x4(Vector3.zero), Path[i + 1].MultiplyPoint3x4(Vector3.zero));

				if (isCircuit)
					Gizmos.DrawLine(Path.Last().MultiplyPoint3x4(Vector3.zero), Path.First().MultiplyPoint3x4(Vector3.zero));
			}

			var maxDistance = Path.GetTotalDistance(isCircuit);

			Vector3 prevPoint = Gizmo_DrawAxes(0, isSelected);
			Vector3 nextPoint;
			for (float dist = gizmoLineSize; dist < maxDistance; dist += gizmoLineSize)
			{
				nextPoint = Gizmo_DrawAxes(dist, isSelected);

				if (isSmooth)
				{
					Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);
					Gizmos.DrawLine(prevPoint, nextPoint);
				}

				prevPoint = nextPoint;
			}
			nextPoint = Gizmo_DrawAxes(maxDistance, isSelected);

			if (isSmooth)
			{
				Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);
				Gizmos.DrawLine(prevPoint, nextPoint);
			}

			if (isSelected)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(
					GetPoint(gizmoPathPoint * maxDistance),
					0.5f);
			}
		}

		private Vector3 Gizmo_DrawAxes(float dist, bool isSelected)
		{
			GetAxesFollowing(dist, out Vector3 origin, out Vector3 right, out Vector3 up, out Vector3 forward);

			Gizmos.color = isSelected ? Color.green : new Color(0, 1, 0, 0.5f);
			Gizmos.DrawRay(origin, up * gizmoLineSize);
			Gizmos.color = isSelected ? Color.red : new Color(1, 0, 0, 0.5f);
			Gizmos.DrawRay(origin, right * gizmoLineSize);

			return origin;
		}
#endif

	}

}

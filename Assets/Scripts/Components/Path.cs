using BLINDED_AM_ME.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Specialized;
using BLINDED_AM_ME.Objects;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BLINDED_AM_ME.Components
{
	[ExecuteInEditMode]
	public class Path : NotifyTransform
	{
		[SerializeField]
		[SerializeProperty(nameof(IsSmooth))]
		private bool _isSmooth = true;
		public bool IsSmooth
		{
			get => _isSmooth;
			set => SetProperty(ref _isSmooth, value);
		}

		[SerializeField]
		[SerializeProperty(nameof(IsCircuit))]
		private bool _isCircuit = false;
		public bool IsCircuit
		{
			get => _isCircuit;
			set => SetProperty(ref _isCircuit, value);
		}

		public Objects.Path PathObject { get; } = new Objects.Path();
		public float TotalDistance => PathObject.GetTotalDistance(_isCircuit);

		public event EventHandler PathChanged;

		public Path()
        {
            PathObject.CollectionChanged += (sender, args) => OnPathChanged();
		}

		protected override void Start()
		{
			base.Start();
			UpdatePath();
		}

		protected override void OnChildAdded(Transform child)
		{
			if (!child.TryGetComponent(out NotifyTransform notifyTransform))
				notifyTransform = child.gameObject.AddComponent<NotifyTransform>();

			notifyTransform.PositionChanged += Child_PositionChanged;
			notifyTransform.RotationChanged += Child_RotationChanged;

			base.OnChildAdded(child);
			UpdatePath();
		}
		protected override void OnChildRemoved(Transform child)
		{
			if (child.TryGetComponent(out NotifyTransform notifyTransform))
			{
				notifyTransform.PositionChanged -= Child_PositionChanged;
				notifyTransform.RotationChanged -= Child_RotationChanged;
			}

			base.OnChildRemoved(child);
			UpdatePath();
		}

		private void Child_PositionChanged(object sender, ItemEventArgs<Vector3> args)
		{
			UpdatePath();
		}
		private void Child_RotationChanged(object sender, ItemEventArgs<Quaternion> args)
		{
			UpdatePath();
		}

        public void UpdatePath()
        {
			// Remove
			if (PathObject.Count > transform.childCount)
			{
				var count = PathObject.Count - transform.childCount;
				var index = PathObject.Count - count;
				PathObject.RemoveRange(index, count);
			}

			int i = -1;
			foreach (Transform child in transform)
			{
				i++;
				child.gameObject.name = "Point " + i;

				if (i < PathObject.Count)
					PathObject[i] = child.localToWorldMatrix;
				else
					PathObject.Add(child.localToWorldMatrix);
			}
        }
        
		protected virtual void OnPathChanged()
		{
			PathChanged?.Invoke(this, EventArgs.Empty);
		}

		public Vector3 GetPoint(float distance)
		{
			return PathObject.GetPoint(distance, IsSmooth, IsCircuit);
		}
		public Matrix4x4 GetMatrix(float distance)
        {
			return PathObject.GetMatrix(distance, IsSmooth, IsCircuit);
        }
		public Matrix4x4 GetMatrixRaw(float distance)
        {
			return PathObject.GetMatrixRaw(distance, IsSmooth, IsCircuit);
        }

#if UNITY_EDITOR

		[Range(0.01f, 1.0f)]
		public float gizmoLineSize = 0.5f;

		[Range(0.0f, 1.0f)]
		public float gizmoPathPoint = 0.0f;

        protected override void OnDrawGizmos()
		{
			var childrenCount = transform.childCount;
			for (int i = 0; i < childrenCount; i++)
			{
				if (!Selection.transforms.Contains(transform.GetChild(i)))
				{
					OnDrawGizmosSelected();
					return;
				}
			}

			DrawGizmos(false);
			base.OnDrawGizmos();
		}
		protected override void OnDrawGizmosSelected()
		{
			DrawGizmos(true);
			base.OnDrawGizmosSelected();
		}

		private void DrawGizmos(bool isSelected)
		{
			if (PathObject.Count < 2)
				return;

			var pathPoints = (IList<Vector3>)PathObject;

			if (!IsSmooth)
			{
				Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);

				for (var i = 0; i < PathObject.Count - 1; i++)
					Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);

				if (IsCircuit)
					Gizmos.DrawLine(pathPoints.First(), pathPoints.Last());
			}

			var maxDistance = PathObject.GetTotalDistance(IsCircuit);

			Vector3 prevPoint = Gizmo_DrawAxes(0, isSelected);
			Vector3 nextPoint;
			for (float dist = gizmoLineSize; dist < maxDistance; dist += gizmoLineSize)
			{
				nextPoint = Gizmo_DrawAxes(dist, isSelected);

				if (IsSmooth)
				{
					Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);
					Gizmos.DrawLine(prevPoint, nextPoint);
				}

				prevPoint = nextPoint;
			}
			nextPoint = Gizmo_DrawAxes(maxDistance, isSelected);

			if (IsSmooth)
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
			var matrix = GetMatrix(dist);
			var origin = matrix.MultiplyPoint3x4(Vector3.zero);
			Gizmos.color = isSelected ? Color.green : new Color(0, 1, 0, 0.5f);
			Gizmos.DrawRay(origin, matrix.MultiplyVector(Vector3.up) * gizmoLineSize);
			Gizmos.color = isSelected ? Color.red : new Color(1, 0, 0, 0.5f);
			Gizmos.DrawRay(origin, matrix.MultiplyVector(Vector3.right) * gizmoLineSize);

			return origin;
		}

#endif

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME
{
    [ExecuteInEditMode]
    public class PathPointsComponent : MonoBehaviour
    {
		public bool isSmooth = true;
		public bool isCircuit = false;

		public PathPoints Path { get; } = new PathPoints();
		public float TotalDistance => Path.GetTotalDistance(isCircuit);

		public PathPointsComponent()
        {
            Path.CollectionChanged += Path_CollectionChanged; ;
		}

        public virtual void Start()
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
			Vector3[] points = new Vector3[children.Length];

			for (int i = 0; i < transform.childCount; i++)
			{
				children[i] = transform.GetChild(i);
				children[i].gameObject.name = "point " + i;

				points[i] = children[i].position;
			}

			// Add
			if (Path.Count < points.Length)
			{
				var count = points.Length - Path.Count;
				var index = points.Length - count;

				var values = new Vector3[count];
				for (int i = 0; i < count; i++)
					values[i] = points[index + i];

				Path.AddRange(values);
			}

			// Remove
			if (Path.Count > points.Length)
			{
				var count = Path.Count - points.Length;
				var index = Path.Count - count;
				Path.RemoveRange(index, count);
			}

			// Update
			for (var i = 0; i < points.Length; i++)
				Path[i] = points[i];
		}

		public Vector3 GetPoint(float dist)
		{
			return Path.GetPoint(dist, isSmooth, isCircuit);
		}

#if UNITY_EDITOR
		
		[Range(0.01f, 1f)]
		public float gizmoLineSize = 1.0f;
		
		[Range(0.0f, 1f)]
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

			Gizmos.color = isSelected ? new Color(0, 1, 1, 1) : new Color(0, 1, 1, 0.5f);

			var maxDistance = Path.GetTotalDistance(isCircuit);
			
			if (isSmooth)
			{
				Vector3 prev = GetPoint(0.0f);
				Vector3 next = prev;
				for (float dist = gizmoLineSize; dist < maxDistance; dist = Mathf.Clamp(dist + gizmoLineSize, 0, maxDistance))
				{
					next = GetPoint(dist);
					Gizmos.DrawLine(prev, next);
					prev = next;
				}

				Gizmos.DrawLine(prev, GetPoint(maxDistance));
			}
			else
            {
				for(var i = 0; i < Path.Count-1; i++)
					Gizmos.DrawLine(Path[i], Path[i + 1]);

				if(isCircuit)
					Gizmos.DrawLine(Path.Last(), Path.First());
			}

			if (isSelected)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(
					Path.GetPoint(gizmoPathPoint * maxDistance,
						isSmooth,
						isCircuit), 0.5f);
			}
		}
#endif

	}
}

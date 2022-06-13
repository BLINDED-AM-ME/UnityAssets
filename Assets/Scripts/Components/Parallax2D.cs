using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace BLINDED_AM_ME.Components
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(Renderer))]
	public class Parallax2D : MonoBehaviour2
	{
        [SerializeField]
        [SerializeProperty(nameof(Boundaries))]
        private Rect _boundaries = new Rect(0, 0, 15, 15);
		public Rect Boundaries
		{
			get => _boundaries;
			set => SetProperty(ref _boundaries, value);
		}

		public List<TwoDeeLayer> Layers = new List<TwoDeeLayer>();

		public Parallax2D() { }
        
		protected override void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			var pointZero = transform.position - (Vector3)Boundaries.center;
			var boudary = new Rect(pointZero, Boundaries.size);
			if(boudary.Contains(cam.transform.position))
				AdjustLayers(cam.transform.position);

			base.LateUpdate();
		}

		Vector3 _displacement;
		Vector3 _point;
		void AdjustLayers(Vector3 viewPoint)
		{
			_displacement = viewPoint - transform.position;
			foreach(var layer in Layers)
            {
				_point = _displacement * layer.multipler + (Vector3)layer.offset;
				_point.z = layer.transform.position.z;

				layer.transform.position = _point;
            }
		}

		[System.Serializable]
		public struct TwoDeeLayer
		{
			public Vector2 offset;

			[Range(0.0f, 1.0f)]
			public float multipler;

			public Transform transform;
		}

#if UNITY_EDITOR

		protected override void Reset()
		{
			Layers.Clear();
			for (int i = 0; i < transform.childCount; i++)
			{
				Layers.Add(new TwoDeeLayer()
				{
					transform = transform.GetChild(i),
					multipler = (float)(i + 1) / ((float)transform.childCount + 1.0f)
				});
			}

			base.Reset();
		}
		protected override void OnDrawGizmos()
		{
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
			Gizmos.color = isSelected ? Color.red : new Color(1, 0, 0, 0.5f);

			var pointZero = transform.position - (Vector3)Boundaries.center;
			var pointOne = pointZero + (Vector3)Boundaries.size;

			Gizmos.DrawRay(pointZero, Vector3.up * Boundaries.height);
			Gizmos.DrawRay(pointZero, Vector3.right * Boundaries.width);

			Gizmos.DrawRay(pointOne, Vector3.down * Boundaries.height);
			Gizmos.DrawRay(pointOne, Vector3.left * Boundaries.width);
		}

#endif
	}

}
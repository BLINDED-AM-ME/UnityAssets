using UnityEngine;
using System.Collections;
using System.Linq;

namespace BLINDED_AM_ME._2D{

	[ExecuteInEditMode]
	public class Parallax2D : MonoBehaviour 
	{

		public Transform     TargetCam;
		public Rect Boundaries = new Rect(0, 0, 15, 15);
		public TwoDeeLayer[] Layers;

		// Use this for initialization
		void Start () 
		{
			if( !TargetCam)
				TargetCam = Camera.main.transform;
		}
			
		void LateUpdate()
		{
			if(!TargetCam)
				return;

			var pointZero = transform.position - (Vector3)Boundaries.center;
			var boudary = new Rect(pointZero, Boundaries.size);
			if(boudary.Contains(TargetCam.position))
				AdjustLayers(TargetCam.position);

		}

		Vector3 _displacement;
		Vector3 _point;
		void AdjustLayers(Vector3 viewPoint)
		{
			_displacement = viewPoint - transform.position;
			foreach(var layer in Layers)
            {
				_point = _displacement * layer.multipler;
				_point.z = layer.transform.position.z;

				layer.transform.transform.position = _point;
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

		void Reset()
		{
			if (!TargetCam)
				TargetCam = Camera.main.transform;

			Layers = new TwoDeeLayer[transform.childCount];

			for (int i = 0; i < transform.childCount; i++)
			{
				Layers[i].transform = transform.GetChild(i);
				Layers[i].multipler = (float)(i + 1) / ((float)transform.childCount + 1.0f);
			}
		}

		protected virtual void OnDrawGizmos()
		{

			DrawGizmos(false);
		}

		protected virtual void OnDrawGizmosSelected()
		{
			DrawGizmos(true);
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
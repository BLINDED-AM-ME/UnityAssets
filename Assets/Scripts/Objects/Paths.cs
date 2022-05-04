using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BLINDED_AM_ME.Extensions;

namespace BLINDED_AM_ME
{
	/// <summary> List of Points </summary>
	public class PathPoints : NotifyList<Vector3>
	{
		private float[] _distances = new float[0];

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
			if (Count < 2)
			{
				_distances = new float[0];
				return;
			}

			_distances = new float[Count];

			float accumalation = 0.0f;
			for (int i = 0; i < Count; ++i)
			{
				accumalation += Vector3.Distance(this[i], this[(i + 1) % Count]);
				_distances[i] = accumalation;
			}

			base.OnCollectionChanged(e);
		}

		public float GetTotalDistance(bool isCircuit)
		{
			if (Count < 2)
				return 0.0f;
			else if (isCircuit)
				return _distances[_distances.Length - 1]; // last
			else
				return _distances[_distances.Length - 2]; // 2nd last
		}

		public Vector3 GetPoint(float distance, bool isSmooth, bool isCircuit)
		{
			if (Count == 0)
				return Vector3.zero;
			else if (Count == 1)
				return this[0];

			distance = Math.Max(0.0f, distance);

			var totalDistance = GetTotalDistance(isCircuit);

			if (isCircuit)
				distance %= totalDistance;
			else
				distance = Math.Min(distance, totalDistance);

			// find segment index
			var segmentIndex = 0;
			for (int i = 0; i < _distances.Length; i++)
			{
				if (distance <= _distances[i])
				{
					segmentIndex = i;
					break;
				}
			}

			var interpolation = 0.0f;
			if (segmentIndex == 0)
				interpolation = Mathf.InverseLerp(0, _distances[segmentIndex], distance);
			else
				interpolation = Mathf.InverseLerp(_distances[segmentIndex - 1], _distances[segmentIndex], distance);


			if (isSmooth)
			{
				if (isCircuit)
				{
					segmentIndex = (segmentIndex - 1 + Count) % Count;
					return MathExtensions.Geometry.CatmullRom(
						this[segmentIndex],
						this[(segmentIndex + 1) % Count],
						this[(segmentIndex + 2) % Count],
						this[(segmentIndex + 3) % Count],
						interpolation);
				}
				else
				{
					return MathExtensions.Geometry.CatmullRom(
						this[Math.Max(segmentIndex - 1, 0)],
						this[segmentIndex],
						this[Math.Min(segmentIndex + 1, Count - 1)],
						this[Math.Min(segmentIndex + 2, Count - 1)],
						interpolation);
				}
			}
			else
			{
				if (isCircuit)
				{
					return Vector3.Lerp(
						this[segmentIndex],
						this[(segmentIndex + 1) % Count],
						interpolation);
				}
				else
				{
					return Vector3.Lerp(
						this[segmentIndex],
						this[segmentIndex + 1],
						interpolation);
				}
			}
		}
	}

	/// <summary> Matrices contain origin, rotation, and scale </summary>
	public class PathMatrices : NotifyList<Matrix4x4>
    {
		private float[] _distances = new float[0];

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (Count < 2)
			{
				_distances = new float[0];
				return;
			}

			_distances = new float[Count];

			float accumalation = 0.0f;
			for (int i = 0; i < Count; ++i)
			{
				accumalation += Vector3.Distance(
					this[i].MultiplyPoint3x4(Vector3.zero),
					this[(i + 1) % Count].MultiplyPoint3x4(Vector3.zero));
				_distances[i] = accumalation;
			}

			base.OnCollectionChanged(e);
		}

		public float GetTotalDistance(bool isCircuit)
		{
			if (Count < 2)
				return 0.0f;
			else if (isCircuit)
				return _distances[_distances.Length - 1]; // last
			else
				return _distances[_distances.Length - 2]; // 2nd last
		}

		public Matrix4x4 GetMatrixRaw(float distance, bool isSmooth, bool isCircuit)
		{
			if (Count == 0)
				return Matrix4x4.identity;
			else if (Count == 1)
				return this[0];

			distance = Math.Max(0.0f, distance);

			var totalDistance = GetTotalDistance(isCircuit);

			if (isCircuit)
				distance %= totalDistance;
			else
				distance = Math.Min(distance, totalDistance);

			// find segment index
			var segmentIndex = 0;
			for (int i = 0; i < _distances.Length; i++)
			{
				if (distance <= _distances[i])
				{
					segmentIndex = i;
					break;
				}
			}

			var interpolation = 0.0f;
			if (segmentIndex == 0)
				interpolation = Mathf.InverseLerp(0, _distances[segmentIndex], distance);
			else
				interpolation = Mathf.InverseLerp(_distances[segmentIndex - 1], _distances[segmentIndex], distance);

			if (isSmooth)
			{
				Matrix4x4 A, B, C, D;

				if (isCircuit)
				{
					segmentIndex = (segmentIndex - 1 + Count) % Count;

					A = this[segmentIndex];
					B = this[(segmentIndex + 1) % Count];
					C = this[(segmentIndex + 2) % Count];
					D = this[(segmentIndex + 3) % Count];
				}
				else
				{
					A = this[Math.Max(segmentIndex - 1, 0)];
					B = this[segmentIndex];
					C = this[Math.Min(segmentIndex + 1, Count - 1)];
					D = this[Math.Min(segmentIndex + 2, Count - 1)];
				}

				var rotation = Quaternion.Lerp(
						B.rotation,
						C.rotation,
						interpolation);

				if (!rotation.IsValid())
					rotation = B.rotation;

				return Matrix4x4.TRS(
					MathExtensions.Geometry.CatmullRom(
						A.MultiplyPoint3x4(Vector3.zero),
						B.MultiplyPoint3x4(Vector3.zero),
						C.MultiplyPoint3x4(Vector3.zero),
						D.MultiplyPoint3x4(Vector3.zero),
						interpolation),
					rotation,
					Vector3.one);
			}
			else
			{
				Matrix4x4 A, B;

				if (isCircuit)
				{
					A = this[segmentIndex];
					B = this[(segmentIndex + 1) % Count];
				}
				else
				{
					A = this[segmentIndex];
					B = this[segmentIndex + 1];
				}

				var rotation = Quaternion.Lerp(
						A.rotation,
						B.rotation,
						interpolation);

				if (!rotation.IsValid())
					rotation = A.rotation;

				return Matrix4x4.TRS(
					Vector3.Lerp(
						A.MultiplyPoint3x4(Vector3.zero),
						B.MultiplyPoint3x4(Vector3.zero),
						interpolation),
					rotation,
					Vector3.one);
			}
		}
		
		/// <summary> Following the Path </summary>
		/// <remarks> MultiplyVector(Vector3.forward) will point along the path</remarks>
		/// <returns> a Matrix dependent on the up axis of nearby Matrices </returns>
		public Matrix4x4 GetMatrixFollowing(float distance, bool isSmooth, bool isCircuit)
        {
			if (Count == 0)
				return Matrix4x4.identity;
			else if (Count == 1)
				return this[0];

			distance = Math.Max(0.0f, distance);

			var totalDistance = GetTotalDistance(isCircuit);

			if (isCircuit)
				distance %= totalDistance;
			else
				distance = Math.Min(distance, totalDistance);

			var matrix = GetMatrixRaw(distance, isSmooth, isCircuit);

			Vector3 point = matrix.MultiplyPoint3x4(Vector3.zero);
			Vector3 forward;
			if(totalDistance - distance >= 0.01f)
				forward = GetPoint(distance + 0.01f, isSmooth, isCircuit) - point;
			else
				forward = point - GetPoint(distance - 0.01f, isSmooth, isCircuit);

			forward.Normalize();

			var rotation = forward == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(forward, matrix.MultiplyVector(Vector3.up));
			if (!rotation.IsValid())
				rotation = matrix.rotation;

				return Matrix4x4.TRS(
					point, 
					rotation,
					Vector3.one);
		}

		public Vector3 GetPoint(float distance, bool isSmooth, bool isCircuit)
		{
			return GetMatrixRaw(distance, isSmooth, isCircuit).MultiplyPoint3x4(Vector3.zero);
		}

		/// <summary> Following the Path </summary>
		/// <remarks> forward will point along the path </remarks>
		public void GetAxesFollowing(float distance, bool isSmooth, bool isCircuit, out Vector3 origin, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
			var matrix = GetMatrixFollowing(distance, isSmooth, isCircuit);

			origin = matrix.MultiplyPoint3x4(Vector3.zero);
			right = matrix.MultiplyVector(Vector3.right);
			up = matrix.MultiplyVector(Vector3.up);
			forward = Vector3.Cross(right, up);
		}

		public void GetAxesRaw(float distance, bool isSmooth, bool isCircuit, out Vector3 origin, out Vector3 right, out Vector3 up, out Vector3 forward)
		{
			var matrix = GetMatrixRaw(distance, isSmooth, isCircuit);

			origin = matrix.MultiplyPoint3x4(Vector3.zero);
			right = matrix.MultiplyVector(Vector3.right);
			up = matrix.MultiplyVector(Vector3.up);
			forward = Vector3.Cross(right, up);
		}

		
	}
}

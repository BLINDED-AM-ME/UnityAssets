using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BLINDED_AM_ME.Extensions
{
	public static class UnityExtensions
	{
		/// <summary> For Coroutine to wait for a Task </summary>
		public static IEnumerator WaitForTask(this Task task)
		{
			yield return new WaitUntil(() => task.IsCompleted);
		}

		/// <summary> For Coroutine to wait for a Task </summary>
		/// <remarks> callback is not called if task is Canceled or Faulted </remarks>
		public static IEnumerator WaitForTask(this Task task, Action callback)
		{
			yield return new WaitUntil(() => task.IsCompleted);
			if (!task.IsCanceled && !task.IsFaulted)
				callback();
		}

		/// <summary> For Coroutine to wait for a Task </summary>
		/// <remarks> callback is not called if task is Canceled or Faulted </remarks>
		public static IEnumerator WaitForTask<T>(this Task<T> task, Action<T> callback)
		{
			yield return new WaitUntil(() => task.IsCompleted);

			if (!task.IsCanceled && !task.IsFaulted)
				callback(task.Result);
		}

		/// <summary> Matrix4x4.TRS will throw an error with an invalid Quaternion </summary>
		public static bool IsValid(this Quaternion quaternion)
		{
			if (float.IsNaN(quaternion.x)
			 || float.IsNaN(quaternion.y)
			 || float.IsNaN(quaternion.z)
			 || float.IsNaN(quaternion.w))
				return false;

			return true;
		}

	}
}
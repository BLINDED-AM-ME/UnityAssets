using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BLINDED_AM_ME.Extensions
{
    public static class MiscExtensions
    {
        /// <summary> Match target to source </summary>
        public static void Match<T>(this IList<T> target, IList<T> source)
        {
            // Remove
            var deadItems = target.Where(item => !source.Contains(item)).ToList();
            foreach (var item in deadItems)
                target.Remove(item);

            // Update
            for (var i = 0; i < source.Count; i++)
            {
                if (target.Count <= i)
                    target.Add(source[i]);
                else
                    target[i] = source[i];
            }
        }
    }
}
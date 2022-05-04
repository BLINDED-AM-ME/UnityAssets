using UnityEngine;
using System.Collections;
using UnityEditor;
using BLINDED_AM_ME;
using System.Threading;
using System;

namespace BLINDED_AM_ME.Inspector
{
	[CustomEditor(typeof(Spline))]
	[CanEditMultipleObjects]
	public class Spline_Editor : Editor {

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button(targets.Length > 1 ? "Generate Meshes" : "Generate Mesh"))
				foreach (Spline target in targets)
					target.GenerateMesh();
		}
	}
}
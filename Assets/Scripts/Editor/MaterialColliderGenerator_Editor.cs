using UnityEngine;
using UnityEditor;
using System.Collections;
using BLINDED_AM_ME;
using BLINDED_AM_ME.Components;

namespace BLINDED_AM_ME.Inspector
{
	[CustomEditor(typeof(MaterialColliderGenerator))]
	[CanEditMultipleObjects]
	public class MeshColliderPerMaterial_Editor : Editor{

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Generate Colliders"))
				foreach(MaterialColliderGenerator target in targets)
					target.GenerateColliders();
		}
	}
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using BLINDED_AM_ME;

namespace BLINDED_AM_ME.Inspector
{
	[CustomEditor(typeof(MeshColliderPerMaterial))]
	[CanEditMultipleObjects]
	public class MeshColliderPerMaterial_Editor : Editor{

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Generate Colliders"))
				foreach(MeshColliderPerMaterial target in targets)
					target.GenerateColliders();
		}
	}
}
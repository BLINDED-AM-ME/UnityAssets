using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BLINDED_AM_ME.Extensions;
using System.Threading.Tasks;
using System.Threading;

namespace BLINDED_AM_ME
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class MeshColliderPerMaterial : MonoBehaviour 
	{
		public GameObject Output = null;

		public void GenerateColliders()
		{
			GameObject outPut = Output != null ? Output : this.gameObject;

			var filter = GetComponent<MeshFilter>();
			var renderer = GetComponent<MeshRenderer>();

			if (filter.sharedMesh == null)
				return;

			Mesh sourceMesh = filter.sharedMesh;
			var sourceMaker = sourceMesh.ToMeshMaker();

			var existingColliders = outPut.GetComponents<MeshCollider>();

			var colliderEnum = existingColliders.GetEnumerator();
			var materialEnum = renderer.sharedMaterials.GetEnumerator();
			foreach(var submesh in sourceMaker.Submeshes)
            {
				MeshCollider collider = null;
				if (colliderEnum.MoveNext())
					collider = (MeshCollider)colliderEnum.Current;
				else
					collider = outPut.AddComponent<MeshCollider>();

				var x = new List<List<int>>() { submesh };
				var maker = sourceMaker.ExtractSubmeshes(x);

				var mesh = maker.ToMesh();
				if (materialEnum.MoveNext())
					mesh.name = ((Material)materialEnum.Current).name;

				collider.sharedMesh = mesh;
            }
		}

	}
}
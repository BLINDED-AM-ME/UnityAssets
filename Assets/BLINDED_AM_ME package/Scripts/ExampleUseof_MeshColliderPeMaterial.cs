using UnityEngine;
using System.Collections;
using BLINDED_AM_ME;

public class ExampleUseof_MeshColliderPeMaterial : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray, out hit)){

				Debug.Log(hit.transform.gameObject.name);
			}

		}
	
	}
}

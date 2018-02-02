using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleUseof_Trajectory_Predicted_Path : MonoBehaviour {

	public float initSpeed = 20.0f;
	public float angle = 45.0f;
	public float initHeight = 10.0f;

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {

	}

	private void OnDrawGizmos(){


		Vector3 initVelocity = transform.TransformDirection(BLINDED_AM_ME.Math_Functions.AngleToVector2D(angle)) * initSpeed;

		Vector3[] path = BLINDED_AM_ME.Math_Functions.Trajectory_Predicted_Path(
			transform.position,
			initVelocity,
			Physics.gravity,
			initHeight, 10); 

		Gizmos.DrawLine(transform.position, path[0]);

		for(int i=0; i<path.Length; i++){
			Gizmos.DrawWireSphere(path[i], 0.5f);
			if(i>0)
				Gizmos.DrawLine(path[i-1], path[i]);
		}
			
	}
}

using UnityEngine;
using System.Collections;

public class MouseClick : MonoBehaviour {
	public Camera firstPersonCamera;
	public Camera overheadCamera;
	private GameObject _cube;

	void Update() {

		if (Input.GetMouseButtonDown (0)) {
			Debug.Log ("Pressed left click.");
			
			firstPersonCamera.enabled = false;
			overheadCamera.enabled = true;
			Debug.Log ("Arrived");
		}


		if (Input.GetMouseButtonDown (1)) {
			Debug.Log ("Pressed right click.");
			firstPersonCamera.enabled = true;
			overheadCamera.enabled = false;
		}

		if (Input.GetMouseButtonDown(2)){
			Debug.Log("Pressed middle click.");
			_cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			_cube.transform.position = new Vector3 (10 * Random.value, 10, 10 * Random.value);
			Rigidbody gameObjectsRigidBody = _cube.AddComponent<Rigidbody>(); // Add the rigidbody.
			gameObjectsRigidBody.mass = 5;
	}
	overheadCamera.transform.position = _cube.transform.position;
	}
}


using UnityEngine;
using System.Collections;

public class sphere : MonoBehaviour {

	GameObject _sphere;
	// Update is called once per frame
	void Update () {
		_sphere = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		// _sphere.transform.localScale = Vector3(0.5, 0.5, 0.5);
	}
}
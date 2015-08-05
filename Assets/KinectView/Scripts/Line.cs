using UnityEngine;
using System.Collections;

public class Line : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawRay(new Vector3 (0, 0, 0), new Vector3(10,10,10), Color.red , 2, false);
	}
}

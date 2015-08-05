using UnityEngine;
using System.Collections;

public class movable_object : MonoBehaviour {

	GameObject m_targetObject;
	Vector3 m_targetOrgin;

	// Use this for initialization
	void Start () {
		m_targetObject = GameObject.Find ("MoveMe");
		m_targetOrgin = m_targetObject.transform.position;
	}

	void Update(){
		m_targetObject.transform.position = transform.position;
	}

	void OnDestroy(){
		m_targetObject.transform.position = m_targetOrgin;
	}
	/*// Update is called once per frame
	void Update () {
	}*/
}

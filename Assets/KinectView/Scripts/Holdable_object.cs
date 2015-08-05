using UnityEngine;
using System.Collections;

public class Holdable_object : MonoBehaviour {

	// Use this for initialization
	protected bool hovered_ = false;
	protected bool grabbed_ = false;

	/*public bool IsHovered(){
		return hovered_;
	}*/

	public bool IsGrabbed(){
		return grabbed_;
	}

	public virtual void OnGrab(){
		grabbed_ = true;
	}

	public virtual void OnRelease(){
		grabbed_ = false;
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

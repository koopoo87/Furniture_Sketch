using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using Windows.Kinect;

public class floorPlane : MonoBehaviour {

	public BodySourceManager _bodySourceManager;
	public BodySourceView _bodySourceView;
	public Windows.Kinect.Body _CurrentBody;
	public GameObject _plane;
	public GameObject Probe;
	public GameObject Room;


	private Vector3 probe_position; 
	private Vector3 room_position; 
	private Vector3 adjustment ; 


	private BodyFrame _BodyFrame;
	private Rigidbody _planeRigidBody;
	private GameObject _KinectPlane;
	private Windows.Kinect.Vector4 _vec4;

	void Start() {
	
		/*_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		_plane.GetComponent<Renderer>().material.color = Color.white;  
		_plane.transform.position = new Vector3 (0, 0, 0);*/
		//_KinectPlane.transform.position = new Vector3 (0, 0, 0);
		//_plane.transform.position  = new Vector3 (0, 0, 0);
		_plane.transform.position  = new Vector3 (0, 0, 0);
		adjustment = new Vector3 (2, 0, -8);
		_bodySourceManager.NewFrame += new BodySourceManager.NewFrameEventHandler (this._floorPlaneNewFrame);
	}

	void _floorPlaneNewFrame(object sender, BodyFrame frame) {
		if (_bodySourceManager.getReader () != null) {
			if (frame != null) {

				_vec4 = frame.FloorClipPlane; 
				//Debug.Log(_vec4.W);
				_plane.transform.position = new Vector3 (-4, _vec4.W * (-1), 5);

				Probe.transform.position =_plane.transform.position +adjustment;
				Room.transform.position = _plane.transform.position + adjustment;
			}
		}				
	}

}

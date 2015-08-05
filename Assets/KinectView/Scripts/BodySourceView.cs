using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
	//to grab object
	public float grabObjectDistance = 2.0f;
	public LayerMask grabLayers = ~0;
	protected Vector3 grab_offset;
	public Collider activeObject; 

	//Camera
	//public Camera main_camera;
	public OVRCameraRig main_camera;

    public Material BoneMaterial;
    public GameObject BodySourceManager;
	public Windows.Kinect.Body CurrentBody;
	private bool IsFirstBody = true; 
	private ulong IDToFirstBody;
	private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>(); //Dictionary to Tracking Bodies
    private BodySourceManager _BodyManager;
	private GameObject curMesh;
	private GameObject prevMesh;

	private Color meshColor;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head }

	};
    void Start(){

		//grab_offset = Vector3.zero;
		activeObject = null;
		CurrentBody = null;
		//meshNum = 0;
	}


	public Windows.Kinect.Body getCurrentBodyData(){
		return CurrentBody;
	}



	/*** Update with every frame *****/
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
				//Debug.Log("Null Body");
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
				main_camera.transform.parent = null;
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);


				if(IDToFirstBody == trackingId)
				{

					IsFirstBody = true;
				}
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
				//Debug.Log("Null Body");
                continue;
			}
            

			//main_camera.transform.position = new Vector3 (0, 0.92f, -4.26f);

            if(body.IsTracked)
            {

				if(IsFirstBody)// Start Detecting only one body 
				{
					Debug.Log("It Is First Body, ID: " + body.TrackingId);
					//put rotation 

					CurrentBody = body;
					IDToFirstBody = body.TrackingId;
					IsFirstBody =false;

					if(!_Bodies.ContainsKey(body.TrackingId))
					{
						_Bodies[body.TrackingId] = CreateBodyObject (body.TrackingId);
						//change coordinates
						//_Bodies[body.TrackingId] = changePerspective(_Bodies[body.TrackingId]);
						//_Bodies[body.TrackingId].transform.localScale = new Vector3(-1,1,-1);
						//StartGrabbing(CurrentBody);
					}

				}

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
				getHandCoordinates(CurrentBody);

			}
        }
    }


    /** Body : Draw Joints **/
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {


			if(jt == Kinect.JointType.HandTipLeft | jt == Kinect.JointType.HandTipRight )
			{
				GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

				// Set color and Transparency 
				Color color = jointObj.GetComponent<Renderer>().material.color;
				color = Color.cyan;
				color.a = -0.1f;
				jointObj.GetComponent<Renderer>().material.color = color; 

				//mesh collider also created 
				LineRenderer lr = jointObj.AddComponent<LineRenderer>();
				lr.SetVertexCount(2);
				lr.material = BoneMaterial;
				lr.SetWidth(0.02f, 0.02f);
				jointObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				jointObj.name = jt.ToString();
				jointObj.transform.parent = body.transform;
				
			}
			else if(jt == Kinect.JointType.Head)
			{
				GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//mesh collider also created 
				LineRenderer lr = jointObj.AddComponent<LineRenderer>();

				/*Camera Transform */
				main_camera.transform.parent = jointObj.transform ;
				main_camera.transform.localScale = new Vector3 (1, 1, 1);



				lr.SetVertexCount(2);
				lr.material = BoneMaterial;
				lr.SetWidth(0.02f, 0.02f);
				jointObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
				jointObj.name = jt.ToString();
				jointObj.transform.parent = body.transform;
			}
			else
			{
				GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

				//mesh collider also created 
				LineRenderer lr = jointObj.AddComponent<LineRenderer>();
				lr.SetVertexCount(2);
				lr.material = BoneMaterial;   
				lr.SetWidth(0.02f, 0.02f);
				jointObj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
				jointObj.name = jt.ToString();
				jointObj.transform.parent = body.transform;
			}	
        } 
        
        return body;
    }
    
	/** Body : Draw Line **/
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {


        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString()); // transform.FindChild
			//jointObj.localScale
			jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();

            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
				lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
				//addColliderToLine(lr, jointObj.localPosition, GetVector3FromJoint(targetJoint.Value));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
	


	/* New Code (7/28 ) : Getting Hand Coordinate */
	private Vector3 getHandCoordinates(Kinect.Body body){


		Vector3 HandCoordinates = new Vector3 (0,0,0);
		// Change Cursor Type 

		if (Input.GetMouseButton (0)) { //When Right Clicked 

			Debug.Log ("Right : Mouse Down Called");
			HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);
			Debug.Log ("Pressed right Click At: " + HandCoordinates);
			CreateMesh (HandCoordinates);

		}if (Input.GetMouseButton (1)) { //When Left Clicked

			HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipLeft]);
			CreateMesh (HandCoordinates);
			//grab current meshes and rotate around it 

		}if (Input.GetMouseButton (2)) {

			if(curMesh != null)
			{

				GameObject root = curMesh.transform.root.gameObject;
				Rigidbody rootRigidBody = root.AddComponent<Rigidbody>();
				rootRigidBody.mass = 5;

				//curMesh = null;
				Destroy (curMesh);
			}

		}

		if (Input.GetAxis ("Mouse ScrollWheel")!=0.0) {

			var d = Input.GetAxis("Mouse ScrollWheel");
			//change colors
			if(d > 0f)//scrol up 
			{
				Debug.Log ("Mouse Scroll Up");
			}
			else if (d <0f)// scroll down
			{
				Debug.Log ("Mouse Scroll Down");
			}
		}

		return HandCoordinates;
	}

	private void CreateMesh(Vector3 coordinates)
	{

		curMesh = GameObject.CreatePrimitive (PrimitiveType.Sphere);

		meshColor = Color.black;
		curMesh.GetComponent<Renderer> ().material.color = meshColor;
		//Add position
		curMesh.transform.position = coordinates;
		curMesh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		/* Make Children */ 
		if (prevMesh != null) {
			curMesh.transform.parent = prevMesh.transform.root;
		}

		prevMesh = curMesh;
	}
	
	/* Start Grabbing Object - Added */
	//Calculate Distance
	private Collider GrabSearch(Vector3 leftHandCoordinate,Vector3 rightHandCoordinate ){
		Vector3 grabPosition = (leftHandCoordinate + rightHandCoordinate) / 2.0f;
		float closest_sqr_distance = grabObjectDistance * grabObjectDistance;

		Collider closest = null;
		Collider[] close_things = Physics.OverlapSphere (grabPosition, grabObjectDistance, grabLayers);

		for (int j = 0; j< close_things.Length; ++j) {

			float sqr_distance = (grabPosition-close_things[j].transform.position).sqrMagnitude;
			//float grab_hand_distance = ( leftHandCoordinate - rightHandCoordinate ).sqrMagnitude; // detect certain amount of distance and regard as grabbed
			Debug.Log("Searching closest objects");
			if(close_things[j].GetComponent<Rigidbody>()!=null && sqr_distance < closest_sqr_distance &&  !close_things[j].transform.IsChildOf(transform) && close_things[j].tag != "NotGrabbable")
			{
				Holdable_object holdable = close_things[j].GetComponent<Holdable_object>();
				Debug.Log("Holdable Object detected");

				if(holdable == null || !holdable.IsGrabbed()){
					closest = close_things[j];
					closest_sqr_distance = sqr_distance;
				}

			}
		}
		return closest;
	}
	
	protected void StartGrabbing(Kinect.Body body){
		Debug.Log ("Start Grabbing");
		//Kinect.Joint sourceJoint = body.Joints [Kinect.JointType.HandLeft];
		Vector3 leftHandCoordinate = GetVector3FromJoint (body.Joints [Kinect.JointType.HandLeft]);
		Vector3 rightHandCoordinate = GetVector3FromJoint (body.Joints [Kinect.JointType.HandRight]);
		Vector3 curGrabPosition = (leftHandCoordinate + rightHandCoordinate) / 2.0f;


		activeObject = GrabSearch(leftHandCoordinate, rightHandCoordinate);
		//Debug.Log ("Grab Position: " + curGrabPosition);

		if (activeObject == null) {
			Debug.Log("No Active Object");
			return;
		}
		Holdable_object holdable = activeObject.GetComponent<Holdable_object>();
		grab_offset = Vector3.zero;

		if(holdable == null){
			Vector3 delta_position = activeObject.transform.position -curGrabPosition;
			Ray grab_ray = new Ray (curGrabPosition, delta_position);
			RaycastHit grab_hit;

			if(activeObject.Raycast(grab_ray, out grab_hit, grabObjectDistance))
				grab_offset = activeObject.transform.position - grab_hit.point;
			else
				grab_offset = activeObject.transform.position - curGrabPosition;
		}
		//wow... Quaternion

		if (holdable != null) {
			holdable.OnGrab();
		}

	}

	protected void OnRelease(){
		if (activeObject != null) {
			Holdable_object hold = activeObject.GetComponent<Holdable_object>();
			if(hold != null)
				hold.OnRelease();

			if(hold == null){
				activeObject.GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Infinity;
			}
		}

		activeObject = null;
		StartGrabbing (CurrentBody);
	}

	/* New Code - Finished */
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
    }



}

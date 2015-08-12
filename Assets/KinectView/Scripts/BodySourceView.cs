using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
	//to grab object
	public float grabObjectDistance = 0.01f;
	//public LayerMask grabLayers = ~0;
	protected Vector3 grab_offset;

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
	private Color[] ColorMap;
	private static ArrayList inactivatedCol = new ArrayList();
	private int colorIndex;
	private GameObject drawingMesh;
	private Collider closest;
	private bool OnMove;
	private bool IsReleased;

	// need to make clean code.... 
	private GameObject activeObject; 
	private GameObject parentMesh;


	private GameObject rootGravityReset;
	private Vector3 rootPosition;
	private Quaternion rootRotation;
	//private GameObject 
	//private Color meshColor;

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
		//activeObject = null;
		OnMove = false;
		CurrentBody = null;
		IsReleased = true;
		//ColorMap = new Color[]{Color.black, Color.white, Color.blue, Color.cyan, Color.gray, Color.green, Color.magenta, Color.red, Color.yellow};
		//Color Map for Furniture
		ColorMap = new Color[]{
			Color.black,
			new Color32 (96, 60, 61, 255),
			new Color32 (196, 175, 163, 255),
			new Color32 (228, 206, 160, 255),
			new Color32 (209, 164, 119, 255),
			new Color32 (200, 121, 87, 255),
			new Color32 (199, 196, 82, 255),
			new Color32 (196, 196, 196, 255),
			new Color32 (178, 191, 203, 255),
			new Color32 (115, 126, 136, 255),
			new Color32 (42, 70, 66, 255),
			new Color32 (196, 179, 199, 255),
			new Color32 (175, 138, 177, 255),
			new Color32 (224, 224, 224, 255),
			new Color32 (84, 168, 181, 255),
			new Color32 (90, 124, 155, 255)
		};
		colorIndex = 0;
		closest = null;
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
					}

				}

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
				getHandCoordinates(CurrentBody, _Bodies[body.TrackingId]);

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
				//Color color = jointObj.GetComponent<Renderer>().material.color;
				//color = Color.cyan;
				jointObj.GetComponent<Renderer>().material.color = ColorMap[colorIndex]; 
				drawingMesh = jointObj;
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
				main_camera.transform.position = jointObj.transform.position;
				main_camera.transform.parent = jointObj.transform ;
				//jointObj.transform.parent = main_camera.transform ;
				main_camera.transform.localScale = new Vector3 (1, 1, 1);



				lr.SetVertexCount(2);
				lr.material = BoneMaterial;
				lr.SetWidth(0.02f, 0.02f);

				//jointObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
				jointObj.GetComponent<MeshRenderer>().enabled = false;
				//jointObj.
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
	private Vector3 getHandCoordinates(Kinect.Body body, GameObject bodyObject){


		Vector3 HandCoordinates =  GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);

		//new Vector3 (0,0,0);
		// Change Cursor Type 
		if(Input.GetMouseButtonDown(0)){



			//Edit Again // 
			parentMesh = HoverObject(HandCoordinates, bodyObject);
			ColorSave(parentMesh ,Color.yellow);
			Rigidbody parentRigidbody = parentMesh.GetComponent<Rigidbody>();

		
			
			if( parentMesh != null&& parentRigidbody == null)
			{


				if(prevMesh == null){
					Debug.Log ("No prev Mesh");

				}
				prevMesh.transform.parent = parentMesh.transform;
				//ColorSave(parentMesh ,Color.yellow);
				//parentMesh = null;
			}
		}

		if (Input.GetMouseButton (0)) { //When Right Clicked 


			HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);
			CreateMesh (HandCoordinates, parentMesh);

		}

		if (Input.GetMouseButtonUp (0)) {

			//GameObject parentMesh = HoverObject(HandCoordinates, bodyObject);
			ColorRestore(parentMesh);
			parentMesh = null;
		}



		if (Input.GetMouseButtonDown (1)) { //When Left Clicked 
		
			OnMove = !OnMove;
			if(OnMove == true){

				HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);
				//CreateMesh (HandCoordinates);
				activeObject = GrabObject(HandCoordinates, bodyObject);
			}

			else if(OnMove == false){

				HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);
				OnRelease(activeObject, HandCoordinates);
			}
		}
		/*if (Input.GetMouseButtonUp (1)) {

			HandCoordinates = GetVector3FromJoint (body.Joints [Kinect.JointType.HandTipRight]);
			OnRelease(activeObject, HandCoordinates);
		}*/



		if (Input.GetMouseButtonDown (2)) { // When Clicked the middle button 

			GameObject hoveredObject = HoverObject(HandCoordinates, bodyObject);
			Rigidbody rigid = hoveredObject.GetComponent<Rigidbody>();

			if(hoveredObject != null){

				if(rigid == null){
				    ApplyGravity(hoveredObject);

				}else if(rigid != null){
									
					foreach(Renderer r in hoveredObject.GetComponentsInChildren<Renderer>()){
						inactivatedCol.Add(r.material.color);
					}
					ResetGravity(hoveredObject);
				}
			}
		}

		if (Input.GetAxis ("Mouse ScrollWheel")!= 0.0) {

			var d = Input.GetAxis("Mouse ScrollWheel");
			//change colors
			if(d > 0f)//scroll up 
			{
				Debug.Log ("Mouse Scroll Up");
				if(colorIndex == (ColorMap.Length-1))
					colorIndex = 0;

				colorIndex ++;
				Color color = ColorMap[colorIndex];
				drawingMesh.GetComponent<Renderer>().material.color = color; 
			}
			else if (d <0f)// scroll down
			{
				Debug.Log ("Mouse Scroll Down");
				if(colorIndex == 0)
					colorIndex = ColorMap.Length;
				
			
				colorIndex--;
				Color color = ColorMap[colorIndex];
				drawingMesh.GetComponent<Renderer>().material.color = color; 
			}



		}

		return HandCoordinates;
	}

	
	private GameObject ApplyGravity(GameObject hoveredObject){

		//GameObject root = curMesh.transform.root.gameObject;
		GameObject root = hoveredObject;
		rootPosition = root.transform.position;
		rootRotation = root.transform.rotation;
		Rigidbody rigid = root.GetComponent<Rigidbody> ();
		Rigidbody rootRigidBody = root.AddComponent<Rigidbody> ();
		rootRigidBody.mass = 5;

		//ColorRestore (root);
		Destroy(curMesh);
		
		return root;

	}


	private void ResetGravity(GameObject root){

		root.transform.position = rootPosition;
		root.transform.rotation = rootRotation;
		Destroy (root.GetComponent<Rigidbody>()); //--> Freeze 

	}


	private void CreateMesh(Vector3 coordinates, GameObject parentMesh)
	{

		curMesh = GameObject.CreatePrimitive (PrimitiveType.Sphere);
		curMesh.GetComponent<Renderer> ().material.SetColor("_Color",ColorMap[colorIndex]);

		//Add position
		curMesh.transform.position = coordinates;
		curMesh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

		/* Make Children */ 
		if (prevMesh != null) {

			curMesh.transform.parent = prevMesh.transform.root;

		} else if (prevMesh == null && parentMesh != null) {

			curMesh.transform.parent = parentMesh.transform.root;
		}

		prevMesh = curMesh;
	}


	
	//Color Save & Restore //


	public static void ColorSave(GameObject root, Color col){

		inactivatedCol.Clear();

		foreach(Renderer r in root.GetComponentsInChildren<Renderer>()){
			inactivatedCol.Add(r.material.color);
			r.material.color = col;
		}
	}
	
	
	public static void ColorRestore(GameObject root){
		
		int i = 0;
		
		foreach(Renderer r in root.GetComponentsInChildren<Renderer>()){
			r.material.color = (Color) inactivatedCol[i]; //cast
			i++;
			//r.material.color = inactivatedCol;
		}
	}

	
	/* Start Grabbing Object - Added */
	//Returns root of the Active Object 
	private GameObject HoverObject(Vector3 handCoordinate, GameObject body){

		float closest_sqr_distance = grabObjectDistance * grabObjectDistance;
		Collider[] close_things = Physics.OverlapSphere (handCoordinate, grabObjectDistance);
		GameObject activeObject; 
		
		
		for (int j = 0; j< close_things.Length; ++j) {
			
			float sqr_distance = (handCoordinate-close_things[j].transform.position).sqrMagnitude;
			//Debug.Log("j: "+ j+ " Sqr_distance : " + sqr_distance + "closest sqr distance: " + closest_sqr_distance);
			
			
			if(close_things[j]==null)
				Debug.Log("Close things == null");
			
			if(close_things[j]!=null && sqr_distance < closest_sqr_distance && !close_things[j].transform.IsChildOf(body.transform))
			{
				GameObject candidate = close_things[j].gameObject;
				//Debug.Log("Close things finding");
				
				if(candidate != null){
					closest = close_things[j];
					closest_sqr_distance = sqr_distance;
				}
				
			}
		}
		
		if (closest != null) {
			activeObject = closest.gameObject.transform.root.gameObject; 
			closest = null;
			return activeObject;
		}else{

			Debug.Log("No hovered Object");
			activeObject = null;
			closest = null;
			return activeObject;
		}

	}
	
	private GameObject GrabObject(Vector3 handCoordinate, GameObject body){



	    float closest_sqr_distance = grabObjectDistance * grabObjectDistance;
		Collider[] close_things = Physics.OverlapSphere (handCoordinate, grabObjectDistance);



		for (int j = 0; j< close_things.Length; ++j) {

			float sqr_distance = (handCoordinate-close_things[j].transform.position).sqrMagnitude;
			Debug.Log("j: "+ j+ " Sqr_distance : " + sqr_distance + "closest sqr distance: " + closest_sqr_distance);


			if(close_things[j]==null)
				Debug.Log("Close things == null");

			if(close_things[j]!=null && sqr_distance < closest_sqr_distance && !close_things[j].transform.IsChildOf(body.transform))
			{
				GameObject candidate = close_things[j].gameObject;
				Debug.Log("Holdable Object detected");

				if(candidate != null){
					closest = close_things[j];
					closest_sqr_distance = sqr_distance;
				}

			}
		}

		if (closest != null && IsReleased == true) {

			//GameObject close_root = closest.transform.root.gameObject; // set Root 
			GameObject activeObject = closest.gameObject.transform.root.gameObject; 
			ColorSave(activeObject ,Color.red);
			activeObject.transform.position = handCoordinate;
			activeObject.transform.parent = body.transform.Find(Kinect.JointType.HandTipLeft.ToString());

			IsReleased = false;
			closest = null;
			return activeObject;

		} else {

			Debug.Log("No Objects");
			GameObject close_root = null;
			closest = null;
			return close_root;
		}

	}


	protected void OnRelease(GameObject active, Vector3 handCoordinate){
			
		if (active != null) {
			Debug.Log ("Onrelease");
			active.transform.parent = null;
			ColorRestore(active);
			active.transform.position = handCoordinate;
			IsReleased = true;
		}



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

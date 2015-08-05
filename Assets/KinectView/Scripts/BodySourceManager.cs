using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;
	private BodyFrame _Frame;
	private GameObject _plane; //get plane
	private Windows.Kinect.Vector4 _vec4;

	public delegate void NewFrameEventHandler(object sender, BodyFrame newFrame);

	public event NewFrameEventHandler NewFrame;

    public Body[] GetData()
    {
        return _Data;
    }

	public BodyFrameReader getReader()
	{
		return _Reader;
	}

    void Start () 
    {
		/*_plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		_plane.transform.position = new Vector3 (0, 0, 0);
		*/
		_Sensor = KinectSensor.GetDefault();
		/*Vector3 _CurrentVector = new Vector3 (0, 1, 0);
*/
        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }   
    }

    void Update () 
    {
        if (_Reader != null)
        {
							
			_Frame = _Reader.AcquireLatestFrame();
			if (_Frame != null)
            {
				//_vec4 = _Frame.FloorClipPlane; 
				//Vector3 normalVector = new Vector3 (_vec4.X, _vec4.Y, _vec4.Z);

				if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];

                }
				_Frame.GetAndRefreshBodyData(_Data);

				NewFrame(this, _Frame);

				_Frame.Dispose();
				_Frame = null;
            }
        }    
    }
    
    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }
        
        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }
            
            _Sensor = null;
        }
    }
}

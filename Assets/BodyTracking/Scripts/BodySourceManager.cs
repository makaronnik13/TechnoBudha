using UnityEngine;
using System.Collections;
using Windows.Kinect;
using System.Collections.Generic;
using System.Linq;
using UniRx;

public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    private List<ulong> Bodies = new List<ulong>();

    public ReactiveCommand<Body> OnBodyTracked = new ReactiveCommand<Body>();
    public ReactiveCommand<ulong> OnBodyUntracked = new ReactiveCommand<ulong>();

    public Body[] GetData()
    {
        return _Data;
    }
    

    void Start () 
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();
            
            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }

        OnBodyTracked.Subscribe(body =>
        {
            Debug.Log(body.TrackingId +" tracked" );
        }).AddTo(this);

        OnBodyUntracked.Subscribe(bodyId =>
        {
            Debug.Log(bodyId + " untracked");
        }).AddTo(this);
    }
    
    void Update () 
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                }
                
                frame.GetAndRefreshBodyData(_Data);
                
                frame.Dispose();
                frame = null;
            }  
        }

        Body[] d = GetData();
        if (d!=null)
        {
            foreach (Body body in d)
            {
                if (body != null && body.IsTracked && Bodies.FirstOrDefault(b=>b == body.TrackingId)==0)
                {
                    Bodies.Add(body.TrackingId);
                    OnBodyTracked.Execute(body);
                }
            }

            for (int i = Bodies.Count - 1; i >= 0; i--)
            {
                Body body = d.Where(b=>b.IsTracked).FirstOrDefault(b=>b.TrackingId == Bodies[i]);

                if (body == null)
                {
                    OnBodyUntracked.Execute(Bodies[i]);
                    Bodies.Remove(Bodies[i]);
                }
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

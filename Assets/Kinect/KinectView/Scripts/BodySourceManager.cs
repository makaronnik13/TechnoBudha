using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UniRx;
using System.Linq;

public class BodySourceManager : MonoBehaviour 
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;

    public ReactiveProperty<Body> trackingBody = new ReactiveProperty<Body>(null);

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


          
          
                if ((trackingBody.Value == null || trackingBody.Value.IsTracked==false) && _Data.Where(b=>b.IsTracked).Count()>0)
                {
                    trackingBody.Value = _Data.Where(b => b.IsTracked).ToList()[0];
                }

                if (!_Data.Where(b => b.IsTracked).Contains(trackingBody.Value))
                {
                   trackingBody.Value = _Data.Where(b => b.IsTracked).FirstOrDefault();
                }
                
                frame.Dispose();
                frame = null;
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

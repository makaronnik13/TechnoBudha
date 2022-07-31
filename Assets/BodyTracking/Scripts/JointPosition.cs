using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class JointPosition : MonoBehaviour 
{
    public JointType _jointType;

	public void SetPosition (CameraSpacePoint pos) 
    {
        transform.localPosition = new Vector3(pos.X*5, pos.Y*5, pos.Z*5);
	}
}
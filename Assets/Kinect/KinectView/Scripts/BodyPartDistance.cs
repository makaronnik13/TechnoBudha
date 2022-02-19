using System;
using UnityEngine;

[Serializable]
public class BodyPartDistance
{

    public Windows.Kinect.JointType J1, J2;

    public float needDistance;


    public void Check(Windows.Kinect.Body body)
    {
        Windows.Kinect.CameraSpacePoint p1 = body.Joints[J1].Position;
        Windows.Kinect.CameraSpacePoint p2 = body.Joints[J2].Position;

        Vector3 v1 = new Vector3(p1.X, p1.Y, p1.Z);
        Vector3 v2 = new Vector3(p2.X, p2.Y, p2.Z);

        float dist = Vector3.Distance(v1, v2);

        Debug.Log(J1  + "/" + J2 + "______" + dist);
    }
}
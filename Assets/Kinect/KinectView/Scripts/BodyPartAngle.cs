using System;
using UnityEngine;

[Serializable]
public class BodyPartAngle
{
    public Windows.Kinect.JointType J1, J2, M;

    public float needAngle;
 

    public void Check(Windows.Kinect.Body body)
    {
        Windows.Kinect.CameraSpacePoint p1 = body.Joints[J1].Position;
        Windows.Kinect.CameraSpacePoint p2 = body.Joints[J2].Position;
        Windows.Kinect.CameraSpacePoint m = body.Joints[M].Position;

        Vector3 v1 = new Vector3(p1.X-m.X, p1.Y-m.Y, p1.Z-m.Z);
        Vector3 v2 = new Vector3(p2.X - m.X, p2.Y - m.Y, p2.Z - m.Z);

        float angle = Vector3.Angle(v1, v2);

        Debug.Log(J1+"/"+m+"/"+J2+"______"+angle);
    }
}
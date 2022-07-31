using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Windows.Kinect;

public class BodyView : MonoBehaviour
{
    private List<JointPosition> joints = new List<JointPosition>();

    private BodySourceManager _bodySourceManager;
    private ulong trackingId;

    private void Awake()
    {
        joints = GetComponentsInChildren<JointPosition>().ToList();
    }

    public void Init(BodySourceManager bodySourceManager, ulong trackingId)
    {
        this.trackingId = trackingId;
        _bodySourceManager = bodySourceManager;
    }

    void Update()
    {
        if (_bodySourceManager == null)
        {
            return;
        }

        //get available bodies
        Body[] data = _bodySourceManager.GetData();
        if (data == null)
        {
            return;
        }


        //process body
        Body body = data.FirstOrDefault(d => d.TrackingId == trackingId && d.IsTracked);

        if (body == null)
        {
            return;
        }

        foreach (JointPosition jp in joints)
        {
            CameraSpacePoint pos = body.Joints[jp._jointType].Position;
            jp.SetPosition(pos);
        }
    }

    public Transform GetJoint(JointType jointType)
    {
        JointPosition jp = joints.FirstOrDefault(j => j._jointType == jointType);
        if (jp == null)
        {
            return null;
        }
        return jp.transform;
    }
}

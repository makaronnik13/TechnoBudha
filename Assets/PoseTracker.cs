using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;

public class PoseTracker : MonoBehaviour
{
    [SerializeField]
    private BodySourceManager bodySourceManager;

    [SerializeField]
    private Slider progressSlider;

    public ReactiveProperty<bool> poseTracked = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> bodyTracked = new ReactiveProperty<bool>(false);
    private ReactiveProperty<float> trackingVolume = new ReactiveProperty<float>(0);

    private IDisposable checkPoseDisposable;

    [SerializeField]
    private PoseAsset Pose;

    private bool recording = false;

    public bool Tracking = true;

    private void Start()
    {
        trackingVolume.Subscribe(v=>
        {
            progressSlider.value = v;

            if (v >= 1)
            {
                poseTracked.Value = true;
                trackingVolume.Value = 0;
            }

            poseTracked.Value = false;
        }).AddTo(this);

        bodySourceManager.trackingBody.Subscribe(body=>
        {
            if (recording)
            {
                return;
            }
            if (checkPoseDisposable!=null)
            {
                checkPoseDisposable.Dispose();
                checkPoseDisposable = null;
            }

            bodyTracked.Value = body != null;


            if (body!=null)
            {
               checkPoseDisposable = Observable.EveryUpdate().Subscribe(_=>
               {
                   if (Tracking && !recording)
                   {

                       float difAngle = 0;
                       float difDistance = 0;

                       foreach (BodyPartAngle ang in Pose.Angles)
                       {

                           Windows.Kinect.Joint J1 = bodySourceManager.trackingBody.Value.Joints[ang.J1];
                           Windows.Kinect.Joint J2 = bodySourceManager.trackingBody.Value.Joints[ang.J2];
                           Windows.Kinect.Joint M = bodySourceManager.trackingBody.Value.Joints[ang.M];

                           Vector3 P1 = GetVector3FromJoint(J1);
                           Vector3 P2 = GetVector3FromJoint(J2);
                           Vector3 PM = GetVector3FromJoint(M);


                           float angle = Vector3.SignedAngle(P1 - PM, P2 - PM, Vector3.up);


                           //ang.needAngle = Mathf.Lerp(ang.needAngle, angle, Time.deltaTime);
                           difAngle += Mathf.Abs(angle - ang.needAngle);
                       }

                       difAngle /= Pose.Angles.Count;

                       foreach (BodyPartDistance dis in Pose.Disctances)
                       {
                           Windows.Kinect.Joint J1 = bodySourceManager.trackingBody.Value.Joints[dis.J1];
                           Windows.Kinect.Joint J2 = bodySourceManager.trackingBody.Value.Joints[dis.J2];

                           float actual = Vector3.Distance(GetVector3FromJoint(J1), GetVector3FromJoint(J2));
                           difDistance += Mathf.Abs(dis.needDistance - actual);
                       }
                       difDistance /= Pose.Disctances.Count;

                       Debug.Log(difDistance+"/"+difAngle);

                       if (difDistance < 0.5f && difAngle < 15)
                       {
                           trackingVolume.Value += Time.deltaTime * 0.5f;
                       }
                       else
                       {
                           trackingVolume.Value -= Time.deltaTime * 0.3f;
                       }

                       trackingVolume.Value = Mathf.Clamp(trackingVolume.Value, 0f, 1f);

                   }
               });
            }
            else
            {
                trackingVolume.Value = 0;
            }
        });
    }

    private Vector3 GetVector3FromJoint(Windows.Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
    }

    [ContextMenu("record")]
    void Record()
    {
        if (bodySourceManager.trackingBody.Value == null)
        {
            return;
        }

        recording = true;
        IDisposable disp = Observable.Timer(System.TimeSpan.FromSeconds(0.1f))
        .Repeat().Subscribe(_ =>
        {
            foreach (BodyPartAngle ang in Pose.Angles)
            {
                Windows.Kinect.Joint J1 = bodySourceManager.trackingBody.Value.Joints[ang.J1];
                Windows.Kinect.Joint J2 = bodySourceManager.trackingBody.Value.Joints[ang.J2];
                Windows.Kinect.Joint M = bodySourceManager.trackingBody.Value.Joints[ang.M];

                Vector3 P1 = GetVector3FromJoint(J1);
                Vector3 P2 = GetVector3FromJoint(J2);
                Vector3 PM = GetVector3FromJoint(M);


                float angle = Vector3.SignedAngle(P1 - PM, P2 - PM, Vector3.up);

                ang.needAngle = Mathf.Lerp(ang.needAngle, angle, 0.5f);
            }

            foreach (BodyPartDistance dis in Pose.Disctances)
            {
                Windows.Kinect.Joint J1 = bodySourceManager.trackingBody.Value.Joints[dis.J1];
                Windows.Kinect.Joint J2 = bodySourceManager.trackingBody.Value.Joints[dis.J2];

                float actual = Vector3.Distance(GetVector3FromJoint(J1), GetVector3FromJoint(J2));

                dis.needDistance = Mathf.Lerp(dis.needDistance, actual, 0.5f);
            }
        });

        Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_=>
        {
            disp.Dispose();
            recording = false;
        }).AddTo(this);
    }

    private void Update()
    {
        progressSlider.gameObject.SetActive(Tracking);
    }
}

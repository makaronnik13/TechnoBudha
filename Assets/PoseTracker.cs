using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using System.Linq;

public class PoseTracker : MonoBehaviour
{
    [SerializeField]
    private BodyVisualizer bodyVisualizer;

    [SerializeField]
    private Slider progressSlider;

    public float TrackTime = 1f;
    public float UntrackTime = 1f;

    public ReactiveProperty<bool> poseTracked = new ReactiveProperty<bool>(false);
    public ReactiveProperty<bool> bodyTracked = new ReactiveProperty<bool>(false);
    private ReactiveProperty<float> trackingVolume = new ReactiveProperty<float>(0);

    [SerializeField]
    private PoseAsset Pose;

    private BodyView trackedBody;

    private bool recording = false;

    public bool Tracking = true;

    private void Start()
    {
        trackingVolume.Subscribe(v =>
        {
            progressSlider.value = v;

            if (v >= 1)
            {
                poseTracked.Value = true;
                trackingVolume.Value = 0;
            }

            poseTracked.Value = false;
        }).AddTo(this);

        bodyVisualizer.OnFirstEntered.Subscribe(b =>
        {
            trackedBody = b;
        }).AddTo(this);

        bodyVisualizer.OnLastExit.Subscribe(_ =>
        {
            trackedBody = null;
        }).AddTo(this);
    }


    [ContextMenu("record")]
    void Record()
    {
        if (trackedBody == null)
        {
            return;
        }

        recording = true;
        IDisposable disp = Observable.Timer(System.TimeSpan.FromSeconds(0.1f))
        .Repeat().Subscribe(_ =>
        {
            foreach (BodyPartAngle ang in Pose.Angles)
            {
                Transform J1 = trackedBody.GetJoint(ang.J1);
                Transform J2 = trackedBody.GetJoint(ang.J2);
                Transform M = trackedBody.GetJoint(ang.M);
                float angle = Vector3.SignedAngle(J1.position - M.position, J2.position - M.position, Vector3.up);

                ang.needAngle = Mathf.Lerp(ang.needAngle, angle, 0.5f);
            }

            foreach (BodyPartDistance dis in Pose.Disctances)
            {
                Transform J1 = trackedBody.GetJoint(dis.J1);
                Transform J2 = trackedBody.GetJoint(dis.J2);

                float actual = Vector3.Distance(J1.position, J2.position);

                dis.needDistance = Mathf.Lerp(dis.needDistance, actual, 0.5f);
            }
        });

        Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_ =>
        {
            disp.Dispose();
            recording = false;
        }).AddTo(this);
    }

    private void Update()
    {
        progressSlider.gameObject.SetActive(Tracking);

        ProcessGesture();
    }

    private void ProcessGesture()
    {
        bodyTracked.Value = trackedBody != null;

        if (trackedBody == null || recording)
        {
            trackingVolume.Value = 0;
            return;
        }

        if (Tracking && !recording)
        {

            float difAngle = 0;
            float difDistance = 0;

            foreach (BodyPartAngle ang in Pose.Angles)
            {

                Transform J1 = trackedBody.GetJoint(ang.J1);
                Transform J2 = trackedBody.GetJoint(ang.J2);
                Transform M = trackedBody.GetJoint(ang.M);
                float angle = Vector3.SignedAngle(J1.position - M.position, J2.position - M.position, Vector3.up);

                //ang.needAngle = Mathf.Lerp(ang.needAngle, angle, Time.deltaTime);
                difAngle += Mathf.Abs(angle - ang.needAngle)*ang.weight;
            }

            difAngle /= Pose.Angles.ToList().Select(a=>a.weight).Sum();

            foreach (BodyPartDistance dis in Pose.Disctances)
            {
                Transform J1 = trackedBody.GetJoint(dis.J1);
                Transform J2 = trackedBody.GetJoint(dis.J2);

                float actual = Vector3.Distance(J1.position, J2.position);
                difDistance += Mathf.Abs(dis.needDistance - actual)*dis.weight;
            }
            difDistance /= Pose.Disctances.ToList().Select(a => a.weight).Sum();

      

            if (difDistance < 0.5f && difAngle < 15)
            {
                trackingVolume.Value += Time.deltaTime * (1f/TrackTime);
            }
            else
            {
                trackingVolume.Value -= Time.deltaTime * (1f/UntrackTime);
            }

            trackingVolume.Value = Mathf.Clamp(trackingVolume.Value, 0f, 1f);

        }
    }
}

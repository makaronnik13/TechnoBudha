using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Windows.Kinect;
using System;
using System.Linq;

public class BodyVisualizer : MonoBehaviour
{
    [SerializeField]
    private BodySourceManager bodySource;
    [SerializeField]
    private GameObject bodyPrefab;

    private Dictionary<ulong, BodyView> bodies = new Dictionary<ulong, BodyView>();

    public ReactiveCommand<BodyView> OnFirstEntered = new ReactiveCommand<BodyView>();
    public ReactiveCommand OnLastExit = new ReactiveCommand();
    public ReactiveCommand<BodyView> OnRemoved = new ReactiveCommand<BodyView>();

    public ReactiveProperty<BodyView> ActiveView = new ReactiveProperty<BodyView>();

    private void Start()
    {
        bodySource.OnBodyTracked.Subscribe(body=>CreateBody(body)).AddTo(this);
        bodySource.OnBodyUntracked.Subscribe(body=>RemoveBody(body)).AddTo(this);
    }

    private void RemoveBody(ulong bodyId)
    {
        if (bodies.ContainsKey(bodyId))
        {
            BodyView bv = bodies[bodyId];

            bodies.Remove(bodyId);

            if (bv == ActiveView.Value)
            {
                ActiveView.Value = bodies.FirstOrDefault().Value;
            }

            Destroy(bv.gameObject);
            if (bodies.Count == 0)
            {
                OnLastExit.Execute();
            }
        }
    }

    private void CreateBody(Body body)
    {
        GameObject newBody = Instantiate(bodyPrefab);
        newBody.transform.SetParent(transform);
        newBody.transform.localScale = Vector3.one;
        newBody.transform.localPosition = Vector3.zero;
        BodyView bv = newBody.GetComponent<BodyView>();
        bv.Init(bodySource, body.TrackingId);
        bodies.Add(body.TrackingId, bv);
        if (bodies.Count == 1)
        {
            OnFirstEntered.Execute(bv);
        }
        if (ActiveView.Value == null)
        {
            ActiveView.Value = bv;
        }
    }

    public BodyView GetBody()
    {
        return bodies.OrderBy(b=>Guid.NewGuid()).FirstOrDefault().Value;
    }
}

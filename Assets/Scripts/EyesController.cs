using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class EyesController : MonoBehaviour
{
    [SerializeField]
    private float RotationSpeed = 0.3f;


    [SerializeField]
    private float Force = 1f;

    [SerializeField]
    private Animator Animator;
    [SerializeField]
    private BodyVisualizer bodyVisualizer;

    private Vector3 lookPos;



    private float time;
    [SerializeField]
    private AnimationCurve DeepCurve;
    private float maxTime;

    // Start is called before the first frame update
    void Start()
    {
        bodyVisualizer.ActiveView.Subscribe(bv=>
        {
            if (bv)
            {
                lookPos = bv.GetJoint(Windows.Kinect.JointType.Head).position;
            }
        }).AddTo(this);

     
        //transform.localScale = Vector3.one * UnityEngine.Random.Range(0.2f, 1.5f);
        AddForce();

        maxTime = DeepCurve.keys.OrderBy(K => K.time).FirstOrDefault().time;
        time = UnityEngine.Random.value * maxTime;
    }

   public void Blink()
    {
      
                if (Animator.GetBool("Open"))
                {
                    Animator.SetTrigger("Blink");
                }

              /*  
        if (Animator.GetFloat("Focus")>0f)
        {
            Animator.SetFloat("Focus", 0, 0.5f, 0.5f);
        }
        else
        {
            Animator.SetFloat("Focus", 1, 0.5f, 0.5f);
        }*/
        
       
    }

    // Update is called once per frame
    void Update()
    {
        if (bodyVisualizer.ActiveView.Value!=null)
        {
            lookPos = Vector3.Lerp(lookPos, bodyVisualizer.ActiveView.Value.GetJoint(Windows.Kinect.JointType.Head).position, Time.deltaTime*RotationSpeed);
            transform.LookAt(lookPos);
            //GetComponent<Rigidbody>().AddForce(transform.forward*Time.deltaTime*DeepCurve.Evaluate(time));
        }

        time += Time.deltaTime;

        if (time>=maxTime)
        {
            time = 0;
        }
    }

    public void AddForce()
    {
        //GetComponent<Rigidbody>().AddForce(UnityEngine.Random.insideUnitCircle.normalized*Force);
    }

    public void Open()
    {
        Animator.SetBool("Open", true);
    }


    public void Close()
    {
        Animator.SetBool("Open", false);
    }
}

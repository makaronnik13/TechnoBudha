using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableAnimation : MonoBehaviour
{
    private Material material;

    private void Awake()
    {
        material = new Material(GetComponent<Renderer>().material);    
    }
    public void Show()
    {
        StartCoroutine(ShowCoroutine(1f));
    }

    private IEnumerator ShowCoroutine(float v)
    {
        material.color = new Color(1,1,1,0);
        float t = v;
        while (v>0)
        {
            material.color = new Color(1, 1, 1, 1-(v/t));
            v -= Time.deltaTime;
            yield return null;
        }
    }

    public void Hide()
    {
        StartCoroutine(HideCoroutine(1f));
    }

    private IEnumerator HideCoroutine(float v)
    {
        material.color = new Color(1, 1, 1, 1);
        float t = v;
        while (v > 0)
        {
            material.color = new Color(1, 1, 1,  (v / t));
            v -= Time.deltaTime;
            yield return null;
        }
    }
}

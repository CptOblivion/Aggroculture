using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public float FlickerSpeed;
    public float flickerLow;
    public float flickerHigh;
    public float[] iterations = new float[] { 2, 5, 2.516f };
    public bool UnscaledTime = false;
    float value;

    void Start()
    {
    }

    void Update()
    {
        float t;
        if (UnscaledTime) t = Time.unscaledTime;
        else t = Time.time;
        value = (flickerHigh + flickerLow)/2f; //midpoint
        for (int i = 0; i < iterations.Length; i++)
        {
            value += Mathf.Sin(t * 3.14f * (FlickerSpeed/ iterations[i])) * .5f * (1f / iterations.Length) * (flickerHigh - flickerLow);
        }
        GetComponent<Light>().intensity = value;
    }
}

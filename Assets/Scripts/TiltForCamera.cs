using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiltForCamera : MonoBehaviour
{
    /*
     * Stores some data about the object it's on, as well as manages adding and removing itself from the script that actually does the work
     */
    [HideInInspector]
    public Quaternion Rotation;
    [HideInInspector]
    public Vector3 Position;
    public bool OverrideDefaultTipAmount = false;
    public float TipAmount = 15;
    public float OffsetCenter = 0;
    [HideInInspector]
    public float ExternalOverride = 0;
    [HideInInspector]
    public float TipWeight = 1;

    bool initialized = false;
    private void OnEnable()
    {
        //wait until Start, the first time we activate (to make sure TipObjectForCamera is initialized and read)
        if (initialized)
        {
            TiltObjectsOnCamera.TipObjects.Add(this);
        }
    }
    private void Start()
    {
        initialized = true;
        TiltObjectsOnCamera.TipObjects.Add(this);
    }

    private void OnDisable()
    {
        TiltObjectsOnCamera.TipObjects.Remove(this);
    }
}

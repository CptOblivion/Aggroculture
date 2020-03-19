using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public float TiltAngle = 60;
    public float TwistAngle = 45;
    public float Size = 15;
    public float Distance = 60;
    public Transform target;
    [Range(0,1)]
    public float SmoothSpeed = 0.5f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (target != null)
        {
            transform.rotation = Quaternion.Euler(TiltAngle, TwistAngle, 0);
            Vector3 TargetPosition = target.position - (transform.forward * Distance);
            transform.position = Vector3.Lerp(transform.position, TargetPosition, SmoothSpeed);
        }
    }
}

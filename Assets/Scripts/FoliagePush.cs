using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoliagePush : MonoBehaviour
{
    public float PushStrengthScale = 1;
    public float PushDistanceLimit = 1;
    public float PushSpringiness = 0.1f;
    float StopThreshold = .001f;

    SkinnedMeshRenderer mesh;
    Vector3 PushInput = Vector3.zero;
    Vector3 PushVector;
    Vector3 InputVelocity; //probably not necessary to put this here but hey, a tiny bit less garbage collection to do

    private void Awake()
    {

        mesh = GetComponent<SkinnedMeshRenderer>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CharacterController>()) InputVelocity = other.GetComponent<CharacterController>().velocity;
        else if (other.GetComponent<Rigidbody>()) InputVelocity = other.GetComponent<Rigidbody>().velocity;
        if (InputVelocity.magnitude > PushInput.magnitude)
        {
            PushInput = InputVelocity;
        }
    }

    private void Update()
    {
        if (PushInput.magnitude > 0)
        {
            PushVector += PushInput * PushStrengthScale * Time.deltaTime;
            if (PushVector.magnitude > PushDistanceLimit)
            {
                PushVector = PushVector.normalized * PushDistanceLimit;
            }
        }
        PushInput = Vector3.zero;

        if (PushVector.magnitude > 0)
        {
            PushVector = PushVector * (1.0f - PushSpringiness);
            if (PushVector.magnitude <= StopThreshold) PushVector = Vector3.zero;

            mesh.material.SetVector("_PushVector", new Vector4(PushVector.x, PushVector.y, PushVector.z));
        }
    }
}

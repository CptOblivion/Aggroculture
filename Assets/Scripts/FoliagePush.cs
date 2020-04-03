using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoliagePush : MonoBehaviour
{
    public float PushStrengthScale = 1;
    public float PushDistanceLimit = 1;
    public float PushSpringiness = 0.1f;
    float StopThreshold = .001f;

    SkinnedMeshRenderer sMesh;
    MeshRenderer mMesh;
    Vector3 PushInput = Vector3.zero;
    Vector3 PushVector;
    Vector3 InputVelocity; //probably not necessary to put this here but hey, a tiny bit less garbage collection to do

    private void Awake()
    {

        sMesh = GetComponent<SkinnedMeshRenderer>();
        mMesh = GetComponent<MeshRenderer>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<CharacterController>()) InputVelocity = other.GetComponent<CharacterController>().velocity;
        else if (other.GetComponent<Rigidbody>()) InputVelocity = other.GetComponent<Rigidbody>().velocity;
        ApplyForce(InputVelocity);
    }

    public void ApplyForce(Vector3 InputVelocity)
    {
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

            if (sMesh)
                sMesh.material.SetVector("_PushVector", new Vector4(PushVector.x, PushVector.y, PushVector.z));
            else if (mMesh)
                mMesh.material.SetVector("_PushVector", new Vector4(PushVector.x, PushVector.y, PushVector.z));
        }
    }
}

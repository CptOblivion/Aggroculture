using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    [HideInInspector]
    public bool Colliding = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMain.current.gameObject)
        {
            Colliding = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMain.current.gameObject)
        {
            Colliding = false;
        }
    }
}

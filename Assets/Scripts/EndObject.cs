using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndObject : MonoBehaviour
{
    [Tooltip("If blank, destroy the object the component is on. Otherwise, attempt to destroy the referenced object. Reference in animation events using the name of the targeted object")]
    public GameObject target = null;
    [Tooltip("Positive values are in seconds, 0 is instantly upon activation, negative numbers mean no timer is used and the function must be manually called")]
    public float DestroyTime = -1;
    float DestroyTimer;
    [Tooltip("Instead of destroying the object and handing it off to garbage collection, just disable it for later reuse")]
    public bool Disable = false;
    private void OnEnable()
    {
        DestroyTimer = DestroyTime;
    }
    private void Update()
    {
        if (DestroyTimer >= 0)
        {
            DestroyTimer -= Time.deltaTime;
            if (DestroyTimer <= 0)
                Destroy();
        }
    }
    public void Destroy(string name = "")
    {
        if (target == null && name == "")
        {
            if (Disable)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }
        else if (target != null && name == target.name)
        {
            if (Disable)
                target.SetActive(false);
            else
                Destroy(target);
        }
    }
}

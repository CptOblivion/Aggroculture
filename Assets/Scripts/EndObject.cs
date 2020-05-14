using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndObject : MonoBehaviour
{
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
    public void Destroy()
    {
        if (Disable)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float DestroyTime = -1;

    private void Update()
    {
        if (DestroyTime > 0)
        {
            DestroyTime -= Time.deltaTime;
            if (DestroyTime <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}

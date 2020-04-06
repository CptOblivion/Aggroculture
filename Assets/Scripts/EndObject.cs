using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndObject : MonoBehaviour
{
    public float DestroyTime = -1;
    public bool Disable = false;

    private void Update()
    {
        if (DestroyTime >= 0)
        {
            DestroyTime -= Time.deltaTime;
            if (DestroyTime <= 0)
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeedPluck : MonoBehaviour
{
    public float SpawnChance = .1f;
    public GameObject Monster;
    void Start()
    {
        if (Monster && Random.value <= SpawnChance)
        {
            Monster = Instantiate(Monster, transform.position, Quaternion.Euler(0,180,0) * transform.rotation);
            Destroy(gameObject);
        }
    }
}

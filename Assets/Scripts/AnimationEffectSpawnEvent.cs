using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectSpawnEvent : MonoBehaviour
{
    public string TagName;
    public GameObject effect;

    public void SpawnEffect(string name)
    {
        if (name == TagName) Instantiate(effect, transform.position, transform.rotation);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEffectSpawnEvent : MonoBehaviour
{
  public string TagName;
  public GameObject effect;
  public bool RandomDirection = false;

  public void SpawnEffect(string name)
  {
    if (name == TagName)
    {
      GameObject obj = Instantiate(effect, transform.position, transform.rotation);
      if (RandomDirection)
        obj.transform.Rotate(0, Random.Range(0f, 360f), 0, Space.World);
    }
  }
}

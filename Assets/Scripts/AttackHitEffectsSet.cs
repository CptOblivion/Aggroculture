using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class AttackHitEffect
{
  public string Material = "Default";
  public GameObject Effect;
  public bool PointAtSource = false;
  public bool StayUpright = false;
}
[CreateAssetMenu(fileName = "AttackHitEffectSet_", menuName = "Attack Hit Effect Set")]
public class AttackHitEffectsSet : ScriptableObject
{
  public AttackHitEffect[] HitEffects;

  public void SpawnHitEffect(string Tag, Vector3 Position, Transform source)
  {
    for (int i = 0; i < HitEffects.Length; i++)
    {
      if (HitEffects[i].Material == Tag)
      {
        if (HitEffects[i].PointAtSource)
        {
          GameObject effect = Instantiate(HitEffects[i].Effect, Position, Quaternion.LookRotation(Position - source.position));
          if (!HitEffects[i].StayUpright)
            effect.transform.Rotate(0, 0, Random.Range(-90f, 90f), Space.Self);

        }
        else
        {
          GameObject effect = Instantiate(HitEffects[i].Effect, Position, source.rotation);
          if (!HitEffects[i].StayUpright)
            effect.transform.Rotate(0, 0, Random.Range(-90f, 90f), Space.Self);
        }
        break;
      }
    }
  }
}

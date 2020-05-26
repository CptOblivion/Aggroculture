using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class AttackContainer
{
    public float Knockback = 0;
    public DamageType[] Damage = new DamageType[0];
    [HideInInspector]
    public Vector3 KnockbackCenter = Vector3.zero;
}
public class AttackHitbox : MonoBehaviour
{
    public string AttackName = "Attack1";
    public AttackContainer AttackDetails;
    public Transform OriginOverride;
    public GameObject AttackEffect;
    public bool EffectIsPrefab;
    public AttackHitEffectsSet attackHitEffectDefaults;

    //TODO: Decide if I need HitEffectsOverride
    //for extra fine-tuning of particular attacks without creating more assets, I guess? Probably just extra bloat
    //public AttackHitEffect[] HitEffectsOverride;

    public bool Sphere = false;
    public float Radius = 1;
    public Vector3 HitboxOffset;
    public Vector3 HalfExtents;
    public bool HitOwnLayer = false;
    public float FoliageForce = 100;

    public bool SingleEffects = false;


    private void OnEnable()
    {
        if (AttackEffect && !EffectIsPrefab)
        {
            AttackEffect.transform.SetParent(null);
            AttackEffect.SetActive(false);
        }
    }
    public void CheckHitBox(bool recursive = true)
    {
        Collider[] colliders;
        DamageReceiver damageReceiver;
        MaterialTags materialTags = null;
        int MadeContact = -1;

        int LayerMask = GlobalTools.GenerateLayerMask(gameObject.layer);
        if (!HitOwnLayer) LayerMask &= ~(1 << gameObject.layer);

        if (OriginOverride)
            AttackDetails.KnockbackCenter = OriginOverride.position;
        else
            AttackDetails.KnockbackCenter = transform.position;
        if (AttackEffect)
        {
            if (EffectIsPrefab)
                Instantiate(AttackEffect, transform.position, transform.rotation);
            else
            {
                AttackEffect.transform.SetPositionAndRotation(transform.position, transform.rotation);
                AttackEffect.SetActive(true);
                AttackEffect.GetComponentInChildren<Animation>().Play(PlayMode.StopAll);
                //make an effect component, with an onenable function that resets its values as though it was just instantiated
            }
        }
        colliders = Physics.OverlapBox(transform.TransformPoint(HitboxOffset), HalfExtents, transform.rotation, LayerMask, QueryTriggerInteraction.Collide);
        //Debug.DrawLine(transform.TransformPoint(HitboxOffset), transform.TransformPoint(HitboxOffset) + Vector3.up * 10);
        for (int i = 0; i < colliders.Length; i++)
        {
            damageReceiver = colliders[i].GetComponent<DamageReceiver>();
            if (damageReceiver)
            {
                damageReceiver.Damage(AttackDetails);
                MadeContact = i;
            }
            if (FoliageForce > 0)
            {
                FoliagePush foliagePush = colliders[i].GetComponent<FoliagePush>();
                if (foliagePush)
                {
                    foliagePush.ApplyForce((foliagePush.transform.position - AttackDetails.KnockbackCenter).normalized * FoliageForce);
                }
            }
        }

        if (colliders.Length > 0 && attackHitEffectDefaults)
        {
            if (SingleEffects)
            {
                //we should really only spawn one effect per swing: prioritize hits that dealt damage, and then ones with a defined material, and if not that then just whatever was hit first
                if (MadeContact != -1)
                    materialTags = colliders[MadeContact].GetComponent<MaterialTags>();
                else
                {
                    for (MadeContact = 0; MadeContact < colliders.Length; MadeContact++)
                    {
                        materialTags = colliders[MadeContact].GetComponent<MaterialTags>();
                        if (materialTags)
                            break;
                    }
                }
                if (materialTags)
                {
                    attackHitEffectDefaults.SpawnHitEffect(materialTags.MaterialTag, colliders[MadeContact].ClosestPoint(transform.position), transform);
                }
            }
            else
            {
                for(int i = 0; i < colliders.Length; i++)
                {
                    materialTags = colliders[i].GetComponent<MaterialTags>();
                    if (materialTags)
                    {
                        attackHitEffectDefaults.SpawnHitEffect(materialTags.MaterialTag, colliders[i].ClosestPoint(transform.position), transform);
                    }

                }
            }
        }

        if (recursive)
        {
            foreach (AttackHitbox child in GetComponentsInChildren<AttackHitbox>())
            {
                if (child != this) child.CheckHitBox(false);
            }
        }
    }
    private void OnDestroy()
    {
        if (AttackEffect && !EffectIsPrefab)
        {
            Destroy(AttackEffect);
        }
    }

    public void ShowAttack()
    {
        if (AttackEffect && !EffectIsPrefab)
        {
            AttackEffect.SetActive(true);
        }
    }
    public void HideAttack()
    {
        if (AttackEffect && !EffectIsPrefab)
        {
            AttackEffect.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(.5f,0,0);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(HitboxOffset, HalfExtents*2);
    }
#endif
}

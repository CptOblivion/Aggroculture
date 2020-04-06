using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//add something to render the hitbox in the gui
public class AttackHitbox : MonoBehaviour
{
    public string AttackName = "Attack1";
    public AttackContainer AttackDetails;
    public Transform OriginOverride;
    public GameObject AttackEffect;
    public bool EffectIsPrefab;

    public bool Sphere = false;
    public float Radius = 1;
    public Vector3 HitboxOffset;
    public Vector3 HalfExtents;
    public bool HitOwnLayer = false;
    public float FoliageForce = 100;


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
        Debug.DrawLine(transform.TransformPoint(HitboxOffset), transform.TransformPoint(HitboxOffset) + Vector3.up * 10);
        for (int i = 0; i < colliders.Length; i++)
        {
            damageReceiver = colliders[i].GetComponent<DamageReceiver>();
            if (damageReceiver)
            {
                damageReceiver.Damage(AttackDetails);
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
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(.5f,0,0);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(HitboxOffset, HalfExtents*2);
        
        if (AttackEffect)
        {
            if (!PrefabUtility.IsPartOfPrefabAsset(AttackEffect))
            {
                AttackEffect.SetActive(true);
            }
        }
    }
#endif
}

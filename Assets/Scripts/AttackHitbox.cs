using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//add something to render the hitbox in the gui
public class AttackHitbox : MonoBehaviour
{
    public string AttackName = "Attack1";
    public AttackContainer AttackDetails;
    public Transform OriginOverride;
    public GameObject AttackEffect;

    public bool Sphere = false;
    public float Radius = 1;
    public Vector3 HalfExtents;
    public bool HitOwnLayer = false;
    public float FoliageForce = 100;

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
            Instantiate(AttackEffect, transform.position, transform.rotation);
        }
        colliders = Physics.OverlapBox(transform.position, HalfExtents, transform.rotation, LayerMask, QueryTriggerInteraction.Collide);
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
                if (foliagePush && GlobalTools.PointInHitbox(foliagePush.transform.position, transform, HalfExtents))
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DamageType
{
    public enum DamageTypes {Cutting, Crushing, Skewering, Spicy, Sour, Salty}
    public DamageTypes Type = DamageTypes.Cutting;
    public float Amount = 0;
}

public class DamageReceiver : MonoBehaviour
{
    bool DamagedThisFrame = false;
    [HideInInspector]
    public AttackContainer DamageInput = null;
    [HideInInspector]
    public UnityEvent OnDamaged = new UnityEvent();

    private void Awake()
    {
        DamageInput = null;
    }
    private void LateUpdate()
    {
        if (DamagedThisFrame)
        {
            if (OnDamaged != null) OnDamaged.Invoke();
            DamageInput = null;
            DamagedThisFrame = false;
        }
    }

    public void Damage(AttackContainer attack)
    {
        if (DamageInput == null || attack.Knockback > DamageInput.Knockback) DamageInput = attack;
        DamagedThisFrame = true;
    }
}

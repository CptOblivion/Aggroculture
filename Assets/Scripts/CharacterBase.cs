using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ResistanceArray
{
  //poise-- find a better name
  [Tooltip("knockback = attack force / knockback resistance, flinch at 5, slide at 10, knockdown at 15")]
  public float Knockback = 1;
  public float Cutting = 1;
  public float Crushing = 1;
  public float Skewering = 1;
  public float Spicy = 0;
  public float Sour = 0;
  public float Salty = 0;

  [HideInInspector]
  public float ThresholdFlinch = 5;
  [HideInInspector]
  public float ThresholdSlide = 10;
  [HideInInspector]
  public float ThresholdKnockdown = 15;
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(DamageReceiver))]
[RequireComponent(typeof(Animator))]
public class CharacterBase : MonoBehaviour
{
  public float RunSpeed = 1.5f;

  public float MaxHealth = 10;
  [HideInInspector]
  public float CurrentHealth;
  public GlobalTools.Layers IFramesLayer = GlobalTools.Layers.MonsterIFrames;

  public float GroundSnapDistance = 2.5f;

  public ResistanceArray DamageResistances = new ResistanceArray();
  public AttackHitbox[] Attacks;
  public Renderer[] Renderers;
  public string DamageFlinchStateName = "DamageFlinch";
  public string DamageSlideStateName = "DamageSlide";
  public string DamageKnockdownStateName = "DamageKnockdown";

  public float DamageFadeTime = .2f;
  public float DeathDamageFadeTime = 1.5f;
  public Color DamageColor = new Color(1, 0, 0);
  protected Color[][] BaseColors;
  protected float DamageFadeSpeed = 0;
  protected float DamageFadeTimer;
  protected float SuperArmor = 0;
  protected bool Staggered = false;


  public UnityEvent OnDeath;

  readonly float IFramesTime = .2f;
  protected float IFramesTimer = 0;



  protected bool alive = true;

  protected CharacterController characterController;
  protected bool iFrames = false;
  protected bool CanMove = false;
  protected bool CanTurn = false;
  protected int StartingLayer;
  protected DamageReceiver damageReceiver;
  protected bool InCombat = false;
  protected CharacterBase Targeting = null;
  protected List<CharacterBase> TargetedBy = new List<CharacterBase>();

  [HideInInspector]
  public Animator animator;
  protected virtual void Awake()
  {
    CurrentHealth = MaxHealth;

    characterController = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();

    StartingLayer = gameObject.layer;

    damageReceiver = GetComponent<DamageReceiver>();
    damageReceiver.OnDamaged.AddListener(delegate { ApplyDamage(damageReceiver); });
    BaseColors = new Color[Renderers.Length][];
    for (int i = 0; i < Renderers.Length; i++)
    {
      BaseColors[i] = new Color[Renderers[i].materials.Length];
      for (int m = 0; m < Renderers[i].materials.Length; m++)
      {
        BaseColors[i][m] = Renderers[i].materials[m].color;
      }
    }
  }

  protected virtual void Update()
  {
    if (!TimeManager.Paused)
    {
      if (DamageFadeTimer > 0)
      {
        DamageFadeTimer -= Time.deltaTime * DamageFadeSpeed;
        for (int i = 0; i < Renderers.Length; i++)
        {
          for (int m = 0; m < Renderers[i].materials.Length; m++)
          {
            Renderers[i].materials[m].color = Color.Lerp(BaseColors[i][m], BaseColors[i][m] * DamageColor, DamageFadeTimer);
          }

        }
      }
      if (alive)
      {
        if (iFrames)
        {
          if (IFramesTimer > 0)
          {
            IFramesTimer -= Time.deltaTime;
            if (IFramesTimer <= 0)
            {
              SetIFrames(0);
            }
          }
        }
      }
    }
  }

  public virtual void ApplyDamage(DamageReceiver receiver)
  {
    DamageType.DamageTypes damageType;
    float damageAmount;
    for (int i = 0; i < receiver.DamageInput.Damage.Length; i++)
    {
      damageAmount = receiver.DamageInput.Damage[i].Amount;
      damageType = receiver.DamageInput.Damage[i].Type;
      if (damageType == DamageType.DamageTypes.Cutting)
      {
        damageAmount -= DamageResistances.Cutting;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
      if (damageType == DamageType.DamageTypes.Crushing)
      {
        damageAmount -= DamageResistances.Crushing;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
      if (damageType == DamageType.DamageTypes.Skewering)
      {
        damageAmount -= DamageResistances.Skewering;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
      if (damageType == DamageType.DamageTypes.Spicy)
      {
        damageAmount -= DamageResistances.Spicy;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
      if (damageType == DamageType.DamageTypes.Sour)
      {
        damageAmount -= DamageResistances.Sour;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
      if (damageType == DamageType.DamageTypes.Salty)
      {
        damageAmount -= DamageResistances.Salty;
        if (damageAmount <= 1) damageAmount = 1;
        CurrentHealth -= damageAmount;
      }
    }

    float force = receiver.DamageInput.Knockback / (DamageResistances.Knockback + SuperArmor);

    if (CurrentHealth <= 0)
    {
      SetIFramesDamage(DeathDamageFadeTime);
      Kill(); //ah, time is die
    }
    else if (force < DamageResistances.ThresholdFlinch) //smol hit, no flinch
    {
      Staggered = false;
      SetIFramesTimer(1);
    }
    else //bigge hit, stop what you're doing
    {
      transform.rotation = Quaternion.LookRotation(receiver.DamageInput.KnockbackCenter - transform.position, Vector3.up);
      Staggered = true;
      SuperArmor = 0;
      animator.SetFloat("Speed", 0);
      CanMove = false;
      CanTurn = false;
      SetIFramesDamage(DamageFadeTime);
      if (force < DamageResistances.ThresholdSlide)
        animator.Play(DamageFlinchStateName, 0);
      else if (force < DamageResistances.ThresholdKnockdown)
        animator.Play(DamageSlideStateName, 0);
      else
        animator.Play(DamageKnockdownStateName, 0);
    }
  }
  public void SetIFrames(int input)
  {
    iFrames = input != 0;
    if (iFrames)
    {
      gameObject.layer = (int)IFramesLayer;
    }
    else
    {
      gameObject.layer = StartingLayer;
    }
  }
  public void SetIFramesDamage(float time)
  {
    if (time > 0)
    {
      DamageFadeTimer = 1;
      DamageFadeSpeed = 1f / time;
    }
    SetIFrames(1);
  }

  public void SetIFramesTimer(int flash)
  {
    IFramesTimer = IFramesTime;
    if (flash != 0)
      SetIFramesDamage(DamageFadeTime);
  }

  public void SetSuperArmor(float armor)
  {
    SuperArmor = armor;
  }

  protected Vector3 SnapToGround()
  {
    Vector3 output = Vector3.zero;
    if (!characterController.isGrounded)
    {
      //characterController.enabled = false;
      float feetDist = (characterController.height / 2) - characterController.radius;
      Vector3 feetPos = characterController.transform.position - characterController.transform.up * (feetDist - characterController.center.y);
      if (Physics.SphereCast(feetPos, characterController.radius, -characterController.transform.up, out RaycastHit hit, GroundSnapDistance))
      {
        Debug.DrawLine(feetPos, hit.point);
        float HitSlopeAngle = Vector3.Dot(hit.normal, characterController.transform.up);
        if (HitSlopeAngle > Mathf.Sin(Mathf.Deg2Rad * characterController.slopeLimit) && hit.point.y < feetPos.y)
        {
          float dropDistance = hit.distance;
          output = -characterController.transform.up * (dropDistance - characterController.skinWidth);
        }
      }
      //characterController.enabled = true;
    }
    return output;
  }

  protected void AssignTarget(CharacterBase target)
  {
    if (alive && target.alive)
    {
      Targeting = target;
      InCombat = true;
      target.AddToTargetedBy(this);
    }
  }

  protected virtual void AddToTargetedBy(CharacterBase targeter)
  {
    TargetedBy.Add(targeter);
  }

  protected virtual void RemoveFromTargetedBy(CharacterBase targeter)
  {
    TargetedBy.Remove(targeter);
    if (TargetedBy.Count == 0)
    {
      InCombat = false;
    }
  }

  protected void TargetDied()
  {
    Targeting = null;
  }

  public virtual void Kill()
  {
    CanMove = false;
    CanTurn = false;

    if (Targeting != null)
    {
      Targeting.RemoveFromTargetedBy(this);
    }
    for (int i = 0; i < TargetedBy.Count; i++)
    {
      TargetedBy[i].TargetDied();
    }
    TargetedBy.Clear();
    alive = false;
    damageReceiver.enabled = false;
    OnDeath.Invoke();
  }

  public void PerformAttack(string name)
  {
    for (int i = 0; i < Attacks.Length; i++)
    {
      if (Attacks[i].AttackName == name)
      {
        Attacks[i].CheckHitBox();
        break;
      }
    }
  }
}
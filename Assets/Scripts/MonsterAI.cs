using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIAttack
{
  public string Name = "";
  public PlayerTrigger[] AttackZones;
  public string AnimationTriggerName = "";
  public float Cooldown = 1;
  //[HideInInspector]
  public float CooldownTimer = 0;
}

public class MonsterAI : CharacterBase
{
  public float PreferredCombatDistance = 10;
  public float CombatDistanceMoveThreshold = 1;
  public float TurnSpeed = .2f;
  public AIAttack[] AttackTriggers;
  public float AggroDistance = 30f;

  int CurrentAttack = -1;
  bool spawning = true;
  float TurnSpeedScale = 1;
  Vector3 VecToPlayer;
  int Wait = 0;

  protected override void Awake()
  {
    base.Awake();
    for (int i = 0; i < Renderers.Length; i++)
    {
      Renderers[i].enabled = false;
    }
    animator.SetFloat("Speed", RunSpeed);
  }

  protected override void Update()
  {
    base.Update();
    if (Wait == 0) Wait = 1;
    else if (Wait == 1)
    {
      Wait = 2;
      for (int i = 0; i < Renderers.Length; i++)
      {
        Renderers[i].enabled = true;
      }
    }
    else if (!spawning && !TimeManager.Paused && alive)
    {
      VecToPlayer = PlayerMain.current.transform.position - transform.position;
      if (!InCombat)
      {
        if (VecToPlayer.magnitude <= AggroDistance)
        {
          AssignTarget(PlayerMain.current);
          InCombat = true;
        }
      }
      else
      {
        if (CanTurn)
        {
          transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(VecToPlayer, Vector3.up), TurnSpeed * TurnSpeedScale * Time.deltaTime * 60);
        }
        if (CanMove)
        {

          //might want to loop through all attacks to check validity, then loop through again to decrement cooldowns?
          //on the other hand it probably doesn't matter
          for (int i = 0; i < AttackTriggers.Length; i++)
          {
            if (AttackTriggers[i].CooldownTimer > 0)
            {
              AttackTriggers[i].CooldownTimer -= Time.deltaTime;
            }
            else
            {
              for (int t = 0; t < AttackTriggers[i].AttackZones.Length; t++)
              {
                if (AttackTriggers[i].AttackZones[t].Colliding)
                {
                  animator.SetTrigger(AttackTriggers[i].AnimationTriggerName);
                  CanMove = false;
                  CurrentAttack = i;
                  goto SkipMovement;
                }
              }
            }
          }

          if (VecToPlayer.magnitude > PreferredCombatDistance + CombatDistanceMoveThreshold)
          {
            animator.SetBool("Idle", false);
            animator.SetFloat("Speed", RunSpeed);
          }
          else if (VecToPlayer.magnitude < PreferredCombatDistance - CombatDistanceMoveThreshold)
          {
            animator.SetBool("Idle", false);
            animator.SetFloat("Speed", -RunSpeed);
          }
          else
          {
            animator.SetBool("Idle", true);
          }
        SkipMovement:;
        }
      }
    }
  }

  public void SetCanMove(int intToBool)
  {
    CanMove = intToBool != 0;
    if (CurrentAttack != -1) AttackTriggers[CurrentAttack].CooldownTimer = AttackTriggers[CurrentAttack].Cooldown;
    CurrentAttack = -1;
  }
  public void SetCanTurn(int intToBool)
  {
    CanTurn = intToBool != 0;
  }
  public void ReturnFromDamage()
  {
    SetCanMove(1);
    SetCanTurn(1);
    ScaleTurnSpeed(1);
    SetIFrames(0);
  }
  public void ScaleTurnSpeed(float NewTurnSpeedScale)
  {
    TurnSpeedScale = NewTurnSpeedScale;
  }
  public void Spawned()
  {
    spawning = false;
    CanMove = true;
    CanTurn = true;
  }
}
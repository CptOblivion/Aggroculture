using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExitAnimationEvent : UnityEvent<ExitAnimation>
{
}
public class PlayerAnimTarget : MonoBehaviour
{
  public string Animation;
  public bool MoveToTarget;
  public float MatchStartTime = .01f;
  public float MatchTime = 0;
  public bool Unequip = false;
  //there doesn't appear to be a way to check if our action is currently subscribed to PlayerMain.current.OnInteractFinished, so we'll have to track it ourselves
  bool Subscribed = false;

  [Tooltip("If false, make sure to trigger ManualReturn() at some point (EG at the end of an animation started by OnAnimFinished)")]
  public bool AutomaticallyReturn = true;

  //this is the instance of the script that is running on the current animation state on the player while the player is locked into an interaction (EG lying on a bed)
  ExitAnimation exitAnimation;

  public UnityEvent OnAnimFinished;

  public void PlayAnim()
  {
    PlayerMain.current.InputDisable();
    Animator playerAnim = PlayerMain.current.GetComponent<Animator>();
    playerAnim.SetTrigger(Animation);
    PlayerMain.current.OnInteractFinished.AddListener(AnimFinished);
    Subscribed = true;
    if (MoveToTarget)
    {
      PlayerMain.current.TargetToMatch = transform;
    }
    if (Unequip)
    {
      PlayerMain.current.Unequip();
    }
  }
  public void AnimFinished(ExitAnimation exitAnimationInput)
  {
    //this is messy, there's probably a better way to get this instance of this script and the current instance of ExitAnimation on the player animator to talk to each other
    Subscribed = false;
    PlayerMain.current.OnInteractFinished.RemoveListener(AnimFinished);
    exitAnimation = exitAnimationInput;

    OnAnimFinished.Invoke();
    if (AutomaticallyReturn)
    {
      ManualReturn();
    }
  }
  private void OnDisable()
  {
    if (Subscribed)
    {
      Subscribed = false;
      PlayerMain.current.OnInteractFinished.RemoveListener(AnimFinished);
    }
  }

  public void ManualReturn()
  {
    //unlocks the controls for the player
    exitAnimation.ReturnFromAnimFinished();
  }
}

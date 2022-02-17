using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAnimation : StateMachineBehaviour
{
  bool AnimFinished = false;
  bool AnimFinishedReturn = false;

  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    AnimFinished = false;
    AnimFinishedReturn = false;
  }
  public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
  {
    if (animatorStateInfo.normalizedTime >= 1)
    {
      if (!TimeManager.Paused)
      {
        if (!AnimFinished)
        {
          PlayerMain.current.OnInteractFinished.Invoke(this);
        }
        if (AnimFinishedReturn && ((PlayerMain.current.inputInteract.triggered && PlayerMain.current.inputInteract.ReadValue<float>() > 0) ||
            (PlayerMain.current.inputLastMove == Vector2.zero && PlayerMain.current.inputMove.ReadValue<Vector2>() != Vector2.zero)))
        {
          PlayerMain.current.animator.SetTrigger("ReturnFromAnim");
        }
      }
    }
  }

  public void ReturnFromAnimFinished()
  {
    AnimFinishedReturn = true;
  }
}

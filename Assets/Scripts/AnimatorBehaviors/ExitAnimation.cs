using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAnimation : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animatorStateInfo.normalizedTime >= 1)
        {
            if (!PauseManager.Paused && ((PlayerMain.current.inputInteract.triggered && PlayerMain.current.inputInteract.ReadValue<float>() > 0) || 
                (PlayerMain.current.inputLastMove == Vector2.zero && PlayerMain.current.inputMove.ReadValue<Vector2>() != Vector2.zero)))
            {
                PlayerMain.current.animator.SetTrigger("ReturnFromAnim");
            }
        }
    }
}

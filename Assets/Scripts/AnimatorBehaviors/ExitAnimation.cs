using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitAnimation : StateMachineBehaviour
{
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        if (animatorStateInfo.normalizedTime >= 1)
        {
            if (!PauseManager.Paused && PlayerMove.currentPlayer.inputActions.FindAction("Interact").triggered &&
                PlayerMove.currentPlayer.inputActions.FindAction("Interact").ReadValue<float>() > 0)
            {
                PlayerMove.currentPlayer.animator.SetTrigger("ReturnFromAnim");
            }
        }
    }
}

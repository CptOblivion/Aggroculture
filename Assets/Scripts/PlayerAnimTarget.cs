using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimTarget : MonoBehaviour
{
    public string Animation;
    public bool MoveToTarget;
    public float MatchStartTime = .01f;
    public float MatchTime = 0;
    public bool Unequip = false;
    public void PlayAnim()
    {
        PlayerMove.currentPlayer.AllowNextInput(0);
        Animator playerAnim = PlayerMove.currentPlayer.GetComponent<Animator>();
        if (MoveToTarget)
        {
            playerAnim.SetTrigger(Animation);
            PlayerMove.currentPlayer.TargetToMatch = transform;
        }
        if (Unequip)
        {
            PlayerMove.currentPlayer.Unequip();
        }


    }
}

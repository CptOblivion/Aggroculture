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
        PlayerMain.current.InputDisable();
        Animator playerAnim = PlayerMain.current.GetComponent<Animator>();
        if (MoveToTarget)
        {
            playerAnim.SetTrigger(Animation);
            PlayerMain.current.TargetToMatch = transform;
        }
        if (Unequip)
        {
            PlayerMain.current.Unequip();
        }


    }
}

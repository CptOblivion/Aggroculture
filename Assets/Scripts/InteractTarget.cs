using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractTarget : MonoBehaviour
{
    public PlayerTrigger playerStand;
    public UnityEvent OnInteract;
    public UnityEvent OnHiglight;

    public void Interact()
    {
        if (playerStand)
        {
            if (playerStand.Colliding) OnInteract.Invoke();
        }
        else OnInteract.Invoke();
    }
    public void Highlight()
    {

        if (playerStand)
        {
            if (playerStand.Colliding) OnHiglight.Invoke();
        }
        else OnHiglight.Invoke();
    }

    public void CallOnPlayer(string Function)
    {
        PlayerMain.current.SendMessage(Function);
    }
}

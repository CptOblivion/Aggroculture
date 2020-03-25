using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractTarget : MonoBehaviour
{
    public PlayerTrigger playerStand;
    public UnityEvent OnInteract;

    public void Interact()
    {
        if (playerStand)
        {
            if (playerStand.Colliding) OnInteract.Invoke();
        }
        else OnInteract.Invoke();
    }

    public void CallOnPlayer(string Function)
    {
        PlayerMove.currentPlayer.SendMessage(Function);
    }
}

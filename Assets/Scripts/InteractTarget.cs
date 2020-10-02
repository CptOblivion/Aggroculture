using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractTarget : MonoBehaviour
{
    bool Highlighted = false;
    public UnityEvent OnInteract;
    public UnityEvent OnHiglight;
    //todo: in unity project, move everything over from OnUnHighlight to OnUnhighlight (unnecessary capital H makes readability bad in Unity)
    public UnityEvent OnUnHighlight;
    public UnityEvent OnUnhighlight;

    //TODO: Test if I need a special case unhighlight for OnDestroy and OnDisable (EG if I just want to disable text prompts that were activated, but not play an animation or whatever)
    //like, a QuietUnhighlight() and OnQuietUnhighlight() event (which gets invoked by regular UnHighLight
    private void OnDestroy()
    {
        if (Highlighted)
            UnHighlight();
    }
    private void OnDisable()
    {
        if (Highlighted)
            UnHighlight();
    }
    public void Enable()
    {
        enabled = true;
    }
    public void Disable()
    {
        if (Highlighted)
            UnHighlight();
        enabled = false;
    }
    public void Interact()
    {
        if (enabled)
        {
            OnInteract.Invoke();
        }
    }
    public void Highlight()
    {
        if (enabled)
        {
            OnHiglight.Invoke();
            Highlighted = true;
        }
    }

    //TODO: rename to Unhighlight (will require reattaching things in Unity)
    public void UnHighlight()
    {
        if (enabled)
        {
            OnUnHighlight.Invoke();
        }
        Highlighted = false;
    }

    public void SetInteractTextPrompt(string phrase)
    {
        InteractPrompt.SetInteractText(phrase);
    }

    public void ClearInteractTextPrompt()
    {
        InteractPrompt.ClearInteractText();
    }

    //is this still used?
    public void CallOnPlayer(string Function)
    {
        PlayerMain.current.SendMessage(Function);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickFunctions : MonoBehaviour
{
    public void SelectWithControlType(Selectable selectable)
    {
        if (selectable)
        {
            selectable.Select();
            selectable.Select();
            if (PlayerMain.current.playerInput.currentControlScheme == "Keyboard")
            {
                selectable.OnDeselect(null);
            }
            else
            {
                selectable.OnSelect(null);
            }
        }
    }
}

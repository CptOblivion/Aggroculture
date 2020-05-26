using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssertMenuStructure : MonoBehaviour
{
    public GameObject[] Enable;
    public GameObject[] Disable;
    public Selectable Select;
    private void OnEnable()
    {
        for (int i = 0; i < Enable.Length; i++)
        {
            Enable[i].SetActive(true);
        }
        for (int i = 0; i < Disable.Length; i++)
        {
            Disable[i].SetActive(false);
        }
        if (Select)
        {
            Select.Select();
            if (PlayerMain.current.playerInput.currentControlScheme == "Keyboard")
            {
                Select.OnDeselect(null);
            }
            else
            {
                Select.OnSelect(null);
            }
        }
    }
}

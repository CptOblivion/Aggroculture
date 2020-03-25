using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuCancel : MonoBehaviour
{
    Button CancelButton;
    InputAction cancel;
    private void Start()
    {
        CancelButton = GetComponent<Button>();
        cancel = PauseManager.pauseManager.inputsUI.FindAction("Cancel");
    }
    private void LateUpdate()
    {
        if (EventSystem.current.currentSelectedGameObject == null && cancel.triggered && cancel.ReadValue<float>() > 0)
        {
            CancelButton.onClick.Invoke();
        }
    }
}

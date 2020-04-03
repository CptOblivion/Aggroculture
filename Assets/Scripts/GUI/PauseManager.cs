using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject PauseParent;
    public InputActionAsset inputsAsset;
    public Dropdown windowModeDropdown;
    public bool PauseOnFocusLoss = true;

    public static bool Paused = false;
    public static bool CanPause = true;
    public static PauseManager current;

    [HideInInspector]
    public InputActionMap inputsUI;
    InputAction inputsGameplay;
    private void Awake()
    {
        current = this;
        inputsUI = inputsAsset.FindActionMap("UI");
        inputsGameplay = inputsAsset.FindActionMap("Gameplay").FindAction("Pause");
        inputsUI.Disable();
        PauseParent.SetActive(false);
    }

    private void Update()
    {
        if (!Paused && CanPause)
        {
            if ((inputsGameplay.triggered && inputsGameplay.ReadValue<float>() > 0) || (PauseOnFocusLoss && !Application.isFocused))
            {
                Pause();
            }
        }
    }

    public void ExitToMenu()
    {

    }

    public void ExitToSystem()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Resume()
    {
        if (CanPause)
        {
            current.PauseParent.SetActive(false);
            current.inputsGameplay.Enable();
            current.inputsUI.Disable();
            StartCoroutine(DelayedUnpause());
        }
    }

    public static void Pause()
    {
        if (CanPause)
        {
            Time.timeScale = 0;
            Paused = true;
            current.PauseParent.SetActive(true);
            current.inputsGameplay.Disable();
            current.inputsUI.Enable();
        }
    }

    IEnumerator DelayedUnpause()
    {
        yield return null;
        Time.timeScale = 1;
        Paused = false;
    }

    public void SetWindowModeDropdown()
    {
        if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen) windowModeDropdown.value = 0;
        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) windowModeDropdown.value = 1;
        if (Screen.fullScreenMode == FullScreenMode.Windowed) windowModeDropdown.value = 2;
    }
    public void WindowModeDropdown(Dropdown target)
    {
        if (target.value == 0)
        {
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        else if (target.value == 1)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        else if (target.value == 2)
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
        Debug.Log($"set window mode to {Screen.fullScreenMode.ToString()}");
    }
}

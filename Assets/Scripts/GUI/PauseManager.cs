using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
  public GameObject HardPauseParent;
  public GameObject SoftPauseParent;
  public InputActionAsset inputsAsset;
  public Dropdown windowModeDropdown;
  public bool PauseOnFocusLoss = true;

  public static bool CanPause = true;
  public static PauseManager current;

  [HideInInspector]
  public InputActionMap inputsUI;
  [HideInInspector]
  public InputActionMap inputsGameplay;
  InputAction inputPause;
  InputAction inputSoftPause;
  private void Awake()
  {
    current = this;
    inputsUI = inputsAsset.FindActionMap("UI");
    inputsGameplay = inputsAsset.FindActionMap("Gameplay");
    inputPause = inputsGameplay.FindAction("Pause");
    inputSoftPause = inputsGameplay.FindAction("SoftPause");
    inputsUI.Disable();
    HardPauseParent.SetActive(false);
    SoftPauseParent.SetActive(false);
  }

  private void Update()
  {
    if (!TimeManager.Paused && CanPause)
    {
      if ((inputPause.triggered && inputPause.ReadValue<float>() > 0) || (PauseOnFocusLoss && !Application.isFocused))
      {
        HardPause();
      }
      else if (inputSoftPause.triggered && inputSoftPause.ReadValue<float>() > 0)
      {
        SoftPause();
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
      current.HardPauseParent.SetActive(false);
      current.SoftPauseParent.SetActive(false);
      current.inputsGameplay.Enable();
      current.inputsUI.Disable();
      StartCoroutine(DelayedUnpause());
    }
  }

  public static void HardPause()
  {
    if (CanPause)
    {
      TimeManager.Pause();
      current.HardPauseParent.SetActive(true);
      current.inputsGameplay.Disable();
      current.inputsUI.Enable();
    }
  }
  public static void SoftPause()
  {
    if (CanPause)
    {
      TimeManager.Pause();
      current.SoftPauseParent.SetActive(true);
      current.inputsGameplay.Disable();
      current.inputsUI.Enable();
    }
  }

  IEnumerator DelayedUnpause()
  {
    yield return null;
    TimeManager.Unpause();
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

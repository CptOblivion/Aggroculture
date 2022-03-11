using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_AssignToHotbar : MonoBehaviour
{
  public Texture emptySlotIcon;
  public static Inventory_AssignToHotbar current;
  int InventorySlot = 0;
  //TODO: Rename to HotbarKeyboard (and reassign objects)
  public Button[] HotbarMouse = new Button[4];
  public Button[] HotbarController = new Button[4];
  public GameObject parentKeyboard;
  public GameObject parentController;
  public RawImage selectedInventoryItemIconObject;
  public Canvas assignToHotbarBlocker;
  int selectedHotbarSlot = 0;
  Vector2 DirectionInput;
  Button thisButton;

  [Tooltip("Wait a moment before reading inputs")]
  public float InputDelayIn = .1f;
  float InputDelayInTimer = 0;

  [Tooltip("After making a selection, wait this long before closing the menu")]
  public float InputDelayOut = .5f;
  float InputDelayOutTimer = 0;

  public float SelectThreshold = .2f;
  public float SelectedInventoryItemIconMoveDistanceController = 40;
  public float SelectedInventoryItemIconOffsetKeyboard = 40;

  int? ItemButton;
  private void Awake()
  {
    current = this;
    gameObject.SetActive(false);
    assignToHotbarBlocker.gameObject.SetActive(false);
    thisButton = GetComponent<Button>();
  }
  public static void BeginHotbarSelection(int slot, Vector3 position)
  {
    current.InventorySlot = slot;
    current.ControlsChanged(PlayerMain.current.playerInput);
    PlayerMain.current.playerInput.controlsChangedEvent.AddListener(current.ControlsChanged);
    current.transform.position = position;
    current.selectedHotbarSlot = 0;
    current.InputDelayInTimer = current.InputDelayIn;
    current.gameObject.SetActive(true);
    current.thisButton.Select();
    current.UpdateAssignIcons();
    current.selectedInventoryItemIconObject.transform.position = current.transform.position;
    current.selectedInventoryItemIconObject.gameObject.SetActive(true);
    current.assignToHotbarBlocker.gameObject.SetActive(true);
    current.selectedInventoryItemIconObject.texture = PlayerInventory.Inventory[current.InventorySlot].item.Icon;
  }
  private void Update()
  {
    if (InputDelayInTimer > 0)
    {
      InputDelayInTimer -= Time.unscaledDeltaTime;
    }
    else if (InputDelayOutTimer > 0)
    {

      InputDelayOutTimer -= Time.unscaledDeltaTime;
      if (InputDelayOutTimer <= 0)
      {
        InventoryScreen.ReturnToInventoryScreen(InventorySlot);
        gameObject.SetActive(false);
      }
    }
    else
    {
      ItemButton = null;
      if (PlayerMain.current.playerInput.currentControlScheme == "Keyboard")
      {
        if (PauseManager.current.inputsUI.FindAction("Item1").triggered && PauseManager.current.inputsUI.FindAction("Item1").ReadValue<float>() > 0) ItemButton = 0;
        else if (PauseManager.current.inputsUI.FindAction("Item2").triggered && PauseManager.current.inputsUI.FindAction("Item2").ReadValue<float>() > 0) ItemButton = 1;
        else if (PauseManager.current.inputsUI.FindAction("Item3").triggered && PauseManager.current.inputsUI.FindAction("Item3").ReadValue<float>() > 0) ItemButton = 2;
        else if (PauseManager.current.inputsUI.FindAction("Item4").triggered && PauseManager.current.inputsUI.FindAction("Item4").ReadValue<float>() > 0) ItemButton = 3;
        if (ItemButton != null)
        {
          SelectSlot((int)ItemButton);
          AssignItemToInventorySlot();
        }
      }
      else
      {
        DirectionInput = PauseManager.current.inputsUI.FindAction("Move").ReadValue<Vector2>();
        if (DirectionInput.x < -SelectThreshold) ItemButton = 0;
        else if (DirectionInput.y > SelectThreshold) ItemButton = 1;
        else if (DirectionInput.x > SelectThreshold) ItemButton = 2;
        else if (DirectionInput.y < -SelectThreshold) ItemButton = 3;
        if (ItemButton != null)
        {
          HotbarController[(int)ItemButton].Select();
          selectedInventoryItemIconObject.transform.position = transform.position + new Vector3(DirectionInput.x, DirectionInput.y, 0) * SelectedInventoryItemIconMoveDistanceController;
          if (PauseManager.current.inputsUI.FindAction("Submit").triggered && PauseManager.current.inputsUI.FindAction("Submit").ReadValue<float>() == 0)
          {
            //SelectSlot((int)ItemButton);
            AssignItemToInventorySlot();
          }
        }
        else
        {
          selectedInventoryItemIconObject.transform.position = transform.position;
          thisButton.Select();
        }
      }
    }

  }
  public void OnReleaseSubmit()
  {
    Debug.Log("released");
  }

  public void SelectSlot(int slot)
  {
    selectedHotbarSlot = slot;
    if (PlayerMain.current.playerInput.currentControlScheme == "Keyboard")
    {
      selectedInventoryItemIconObject.transform.position = HotbarMouse[slot].transform.position + new Vector3(0, -SelectedInventoryItemIconOffsetKeyboard, 0);
    }
  }
  public void PointerExit()
  {
    selectedInventoryItemIconObject.transform.position = transform.position;
  }
  public void ControlsChanged(UnityEngine.InputSystem.PlayerInput playerInput)
  {
    //TODO: Maybe on controls changed, just cancel hotbar assignment
    parentKeyboard.SetActive(playerInput.currentControlScheme == "Keyboard");
    parentController.SetActive(!parentKeyboard.activeSelf);
    /*
    for (int i = 0; i < 4; i++)
    {
        HotbarMouse[i].gameObject. SetActive(keyboard);
        HotbarController[i].gameObject.SetActive(!keyboard);
    }
    */
  }

  public void AssignItemToInventorySlot()
  {
    if (PlayerInventory.Hotbar[selectedHotbarSlot] != InventorySlot)
    {
      for (int i = 0; i < PlayerInventory.Hotbar.Length; i++)
      {
        if (PlayerInventory.Hotbar[i] == InventorySlot)
        {
          PlayerInventory.Hotbar[i] = PlayerInventory.Hotbar[selectedHotbarSlot];
          break;
        }
      }

      PlayerInventory.Hotbar[selectedHotbarSlot] = InventorySlot;
      PlayerMain.current.UpdateHotbar();
      UpdateAssignIcons();
    }
    selectedInventoryItemIconObject.gameObject.SetActive(false);
    InputDelayOutTimer = InputDelayOut;
  }
  void UpdateAssignIcons()
  {
    for (int i = 0; i < 4; i++)
    {
      if (PlayerInventory.Hotbar[i] == null)
      {
        current.HotbarController[i].GetComponentInChildren<RawImage>().texture = emptySlotIcon;
        current.HotbarMouse[i].GetComponentInChildren<RawImage>().texture = emptySlotIcon;
      }
      else
      {
        current.HotbarController[i].GetComponentInChildren<RawImage>().texture = PlayerInventory.Inventory[(int)PlayerInventory.Hotbar[i]].item.Icon;
        current.HotbarMouse[i].GetComponentInChildren<RawImage>().texture = PlayerInventory.Inventory[(int)PlayerInventory.Hotbar[i]].item.Icon;
      }
    }
  }

  public void Cancel()
  {
    InventoryScreen.ReturnToInventoryScreen(InventorySlot);
    gameObject.SetActive(false);
  }
  private void OnDisable()
  {
    if (PlayerMain.current)
      PlayerMain.current.playerInput.controlsChangedEvent.RemoveListener(ControlsChanged);
    assignToHotbarBlocker.gameObject.SetActive(false);
  }
}

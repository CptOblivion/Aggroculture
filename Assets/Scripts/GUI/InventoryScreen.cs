using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InventoryScreen : MonoBehaviour
{
    public int? SelectedInventorySlot = null;
    public Button EmptySlot;
    public int InventoryWidth = 6;
    public int InventoryHeight = 4;
    public float GridOffsetX = 100;
    public float GridOffsetY = 100;
    readonly string VerySecureInventoryButtonName = "Inventory Button, Don't Use This Name For Anything Else!";
    public Event cancelEvent;
    Button[,] ImageGrid;
    bool init = false;
    public static InventoryScreen current;
    int? WaitForControlCenter = null;
    public List<int> MarkedSlots = new List<int>();
    public Color EmptySlotColor = Color.gray;
    private void Awake()
    {
        init = true;
        transform.parent.parent.gameObject.SetActive(false);
        current = this;
    }
    void OnEnable()
    {
        if (init)
        {
            int CurrentInventorySlot;
            RawImage SlotTex;
            ImageGrid = new Button[InventoryWidth, InventoryHeight];
            for (int y = 0; y < InventoryHeight; y++)
            {
                for (int x = 0; x < InventoryWidth; x++)
                {
                    CurrentInventorySlot = GetInventoryItemPosition(x, y);
                    int tempCurrentInventorySlot = CurrentInventorySlot;
                    ImageGrid[x, y] = Instantiate(EmptySlot, GetInventoryButtonPosition(x,y), transform.rotation, transform);
                    ImageGrid[x, y].name = VerySecureInventoryButtonName;
                    ImageGrid[x, y].onClick.AddListener(delegate { SelectItem(tempCurrentInventorySlot); });
                    SlotTex = ImageGrid[x, y].GetComponentInChildren<RawImage>();
                    if (PlayerInventory.Inventory.Length > CurrentInventorySlot && PlayerInventory.Inventory[CurrentInventorySlot] != null)
                    {
                        SlotTex.texture = PlayerInventory.Inventory[CurrentInventorySlot].item.Icon;
                        if (PlayerInventory.Inventory[CurrentInventorySlot] != null && PlayerInventory.Inventory[CurrentInventorySlot].item.Stacklimit != 0)
                        {
                            ImageGrid[x, y].GetComponentInChildren<Text>().text = PlayerInventory.Inventory[CurrentInventorySlot].StackSize.ToString();
                        }
                    }
                    else
                    {
                        //ImageGrid[x, y].interactable = false;
                        SlotTex.color = EmptySlotColor;
                    }
                }
            }

            if (ImageGrid[0,0] != null)
            {
                ImageGrid[0, 0].Select();
            }
            if (MarkedSlots.Count > 0)
            {
                string DebugString = "Slots marked as new: ";
                for (int i = 0; i < MarkedSlots.Count; i++)
                {
                    if (i == 0)
                        DebugString += MarkedSlots[i];
                    else
                        DebugString += $", {MarkedSlots[i]}";
                }
                //TODO: Actually indicate marked slots
                Debug.Log(DebugString);
                MarkedSlots.Clear();
            }
        }
    }
    private void Update()
    {
        //when returning from the hotbar assignment, we're selecting nothing until the directional controls are recentered (to avoid accidentally moving the cursor after holding a direction to assign a hotkey
        if (WaitForControlCenter != null)
        {
            if (PauseManager.current.inputsUI.FindAction("Move").ReadValue<Vector2>() == Vector2.zero)
            {
                Vector2Int inventoryArrayPosition = current.GetInventoryArrayPosition((int) WaitForControlCenter);
                current.ImageGrid[inventoryArrayPosition.x, inventoryArrayPosition.y].Select();
                WaitForControlCenter = null;
            }
        }
    }

    Vector3 GetInventoryButtonPosition(int x, int y)
    {
        return transform.position + new Vector3(GridOffsetX / 2f + GridOffsetX * ((float)x - (float)InventoryWidth / 2f), -GridOffsetY / 2f - GridOffsetY * ((float)y - (float)InventoryHeight / 2f), 0);
    }
    Vector3 GetInventoryButtonPosition(int slot)
    {
        Vector2Int inventoryArrayPosition = GetInventoryArrayPosition(slot);
        return GetInventoryButtonPosition(inventoryArrayPosition.x, inventoryArrayPosition.y);
    }

    Vector2Int GetInventoryArrayPosition(int slot)
    {
        int x = slot % InventoryWidth;
        return (new Vector2Int(x, (slot - x) / InventoryWidth));
    }
    int GetInventoryItemPosition(int x, int y)
    {
        return (y * InventoryWidth) + x;
    }

    private void OnDisable()
    {
        if (ImageGrid != null)
        {
            for (int y = 0; y < InventoryHeight; y++)
            {
                for (int x = 0; x < InventoryWidth; x++)
                {
                    Destroy(ImageGrid[x, y].gameObject);
                }
            }
        }
    }
    public void SelectItem(int slot)
    {
        if (PlayerInventory.Inventory.Length > slot && PlayerInventory.Inventory[slot] != null)
        {
            //OnClick on inventory slot activates this function (added in OnEnable, don't do manually)
            Vector2Int index = GetInventoryArrayPosition(slot);
            Inventory_AssignToHotbar.BeginHotbarSelection(slot, ImageGrid[index.x, index.y].transform.position); //this pops up the hotbar around the current inventory item
        }
    }

    public static void ReturnToInventoryScreen(int slot)
    {
        Vector2Int inventoryArrayPosition = current.GetInventoryArrayPosition(slot);
        current.ImageGrid[inventoryArrayPosition.x, inventoryArrayPosition.y].OnSelect(null); //this makes it look like the button is selected, even though it's technically not until the controls are recentered

        current.WaitForControlCenter = slot;
    }

    public static void MarkSlotsAsNew(int[] slots)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            current.MarkedSlots.Add(slots[i]);
        }
    }
}

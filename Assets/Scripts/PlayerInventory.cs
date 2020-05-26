using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int StackSize;
}
public class PlayerInventory : MonoBehaviour
{
    public static InventorySlot[] Inventory;
    public static int?[] Hotbar;
    public static int StackLimitDefault = 64;

    public static void InitializeInventory(InventorySlot[] contents = null, int size = 24)
    {
        if (contents != null)
        {
            Inventory = new InventorySlot[size];
            Hotbar = new int?[] { null, null, null, null };
            for (int i = 0; i < contents.Length; i++)
            {
                Inventory[i] = contents[i];
            }
            for (int i = 0; i < Hotbar.Length; i++)
            {
                if (Inventory.Length > i)
                {
                    Hotbar[i] = i;
                }
            }
        }
        else
        {

            if (Inventory == null)
            {
                Inventory = new InventorySlot[size];
                Hotbar = new int?[] { null, null, null, null };
            }
        }
    }

    public static InventorySlot GetHotbarEntry(int slot)
    {
        //InitializeInventory();
        if (slot < 0 || slot > 3)
        {
            Debug.LogError("Invalid hotbar slot!");
            return null;
        }
        if (Hotbar[slot] == null)
        {
            return null;
        }
        return Inventory[(int)Hotbar[slot]];
    }

    /// <summary>
    /// Attempts to add the given item stack to the inventory- will make a pass to combine stacks first, and failing that will look for an empty slot.
    /// Sets item.stacksize to 0 if the stack was fully deposited into inventory.
    /// Returns an array of the slots that were deposited into, which will be empty if there was no space in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static int[] AddItem(InventoryItem item, int StackSize, out int NewStackSize)
    {
        //TODO: NewStackSize isn't reducing properly when stacking
        NewStackSize = StackSize;
        int StackLimit = GetStackLimit(item.Stacklimit);
        List<int> AddedSlots = new List<int>();

        //first we try to combine into existing stacks, if it's stackable
        if (StackLimit > 1)
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] != null && Inventory[i].item == item && Inventory[i].StackSize < StackLimit)
                {
                    AddedSlots.Add(i);
                    Inventory[i].StackSize += NewStackSize;
                    if (Inventory[i].StackSize > StackLimit)
                    {
                        NewStackSize = Inventory[i].StackSize - StackLimit;
                        Inventory[i].StackSize = StackLimit;
                    }
                    else
                    {
                        NewStackSize = 0;
                        break;
                    }
                }
            }
        }

        //if there's anything left in the stack after combining, look for the first empty slot to put it in
        if (NewStackSize > 0)
        {
            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i] == null)
                {
                    AddedSlots.Add(i);
                    if (NewStackSize > StackLimit)
                    {
                        Inventory[i] = new InventorySlot() { item = item, StackSize = StackLimit };
                        NewStackSize -= StackLimit;

                    }
                    else
                    {
                        Inventory[i] = new InventorySlot() { item = item, StackSize = NewStackSize };
                        NewStackSize = 0;
                        break;
                    }
                }
            }
        }
        return AddedSlots.ToArray();
    }

    public static int GetStackLimit(float StackLimit)
    {
        return (int)(Mathf.Max(StackLimitDefault * StackLimit, 1));
    }
}

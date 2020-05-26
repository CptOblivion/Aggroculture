using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryItemPickup : MonoBehaviour
{
    public InventoryItem item;
    public float PickupDelayTime = 1;
    public float HoverHeight = 1;
    public int StackSize = 1;
    float PickupDelayTimer;
    Collider collider;

    private void Awake()
    {
        collider = GetComponent<Collider>();
    }
    void OnEnable()
    {
        ResetPickupTimer();
        //TODO: Use a raycast instead of a hardcoded height (this is just a quick fix to get the thing implemented)
        transform.position = new Vector3(transform.position.x, HoverHeight, transform.position.z);
    }

    void Update()
    {
        if (PickupDelayTimer > 0)
        {
            PickupDelayTimer -= Time.deltaTime;
            if (PickupDelayTimer > 0)
            {
                collider.enabled = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerMain playerMain = other.GetComponent<PlayerMain>();
        if (playerMain)
        {
            int[] InventorySlots = AddToInventory();
            InventoryScreen.MarkSlotsAsNew(InventorySlots);
            if (StackSize == 0)
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("inventory full");
                ResetPickupTimer();
            }
        }
    }
    public int[] AddToInventory()
    {
        return PlayerInventory.AddItem(item, StackSize, out StackSize);
    }

    public void ResetPickupTimer()
    {
        PickupDelayTimer = PickupDelayTime;
        collider.enabled = false;
    }
}

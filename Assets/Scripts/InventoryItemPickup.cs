using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InventoryItemPickup : MonoBehaviour
{
    public InventoryItem item;
    public float PickupDelayTime = 1;
    public float HoverHeightMin = 1;
    public float HoverHeightMax = 2;
    public float HoverTime = 2;
    public float PopHeight = 4;
    public int BounceCount = 3;
    public float SpinTime = 5;
    float BounceOffset;
    float HoverOffset = 0;

    public int StackSize = 1;
    float PickupDelayTimer;
    Collider collider;

    private void Awake()
    {
        //todo: some means to randomize stack sizes for drops (put that on the monster?)
        collider = GetComponent<Collider>();
    }
    void OnEnable()
    {
        ResetPickupTimer();
        //TODO: Use a raycast instead of a hardcoded height (this is just a quick fix to get the thing implemented)
    }

    void Update()
    {
        if (PickupDelayTimer > 0)
        {
            transform.position = new Vector3(transform.position.x, Pop(PickupDelayTimer / PickupDelayTime, PopHeight - HoverHeightMax)+HoverHeightMin, transform.position.z);
            PickupDelayTimer -= Time.deltaTime;
            if (PickupDelayTimer <= 0)
            {
                collider.enabled = true;
            }
        }
        else
        {
            transform.position = new Vector3(transform.position.x, Hover(), transform.position.z);
        }
        transform.Rotate(Vector3.up, 360 / SpinTime * Time.deltaTime);

    }

    private float Hover()
    {
        HoverOffset += Time.deltaTime;
        return .5f * ((Mathf.Sin((HoverOffset - .5f * HoverTime) * (Mathf.PI / HoverTime)) * (HoverHeightMax - HoverHeightMin)) + (HoverHeightMax + HoverHeightMin));
    }
    //TODO: This was fun to write, but it should probably be replaced with a simple parabola and damping (where if the function dips below 0, lower the height and time and restart it)
    //this might be better in a shader but I think it uses both more processing power and more memory than the other way in a not-so-parallel environment
    private float Pop(float t, float h)
    {
        //return (-Mathf.Pow(t,2) + t) * 4 * h / 2;
        t = 1 - t;
        t = (1 - BounceOffset) * t + BounceOffset;
        //this is just a recurringly smaller sin with an offset since the first half-cycle of the function is wonk
        return Mathf.Abs(Mathf.Sin((BounceCount+1)*Mathf.PI*t*t)) * (1 - t) * 1.45f * h;
    }
    void ComputeBounceOffset()
    {
        //these numbers are kinda random, I just got them by plugging various bounce counts into a chart and plotting graphs until I found the best fit
        BounceOffset = -.46f * Mathf.Log10(.225f * BounceCount) + .00545f * BounceCount + .41f;
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerMain playerMain = other.GetComponent<PlayerMain>();
        if (playerMain)
        {
            int[] InventorySlots = AddToInventory();
            if (InventorySlots.Length > 0)
            {
                playerMain.UpdateHotbar();
            }
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
        HoverOffset = 0;
        ComputeBounceOffset();
    }
}

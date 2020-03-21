using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerMove : MonoBehaviour
{
    public InputActionAsset inputActions;
    public float RunSpeed = 1;
    public FollowCam followCam;
    public InventoryItem[] Hotbar = new InventoryItem[5];
    public GameObject LeftHand;
    public GameObject RightHand;
    public float InteractDistanceFar = 3;
    public float InteractDistanceNear = 1;
    public FarmPlot farmPlot;
    public GameObject debugBuddy;

    public InventoryItem Tools_Empty;

    public static PlayerMove currentPlayer;

    GameObject[] HeldItems = { null, null };
    int ActiveHotbarSlot = 0;
    int ItemButton = 0;
    bool iFrames = false;
    bool QueueNextInput = true;//allow the next input to be queued while the current animation is running (typically enabled near the end of the animation)
    Vector2 Movement = Vector2.zero;
    Animator animator;
    CharacterController characterController;
    InputActionMap actionsGameplay;

    private void Awake()
    {
        currentPlayer = this;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        inputActions.Enable();
        actionsGameplay = inputActions.FindActionMap("Gameplay");
        debugBuddy = Instantiate(debugBuddy);
        debugBuddy.SetActive(false);
    }

    void Update()
    {
        if (QueueNextInput && !PauseManager.Paused)
        {
            if (actionsGameplay.FindAction("Dodge").triggered && actionsGameplay.FindAction("Dodge").ReadValue<float>() > 0) 
            {
                animator.SetTrigger("Dodge");
                iFrames = true;
                goto InputsFinished;
            }

            if (actionsGameplay.FindAction("UseL").ReadValue<float>() > 0)
            {
                if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Hoe)
                {
                    bool hoeing = HoeGround(1);
                    if (hoeing)
                    {
                        StartToolAnim("UseHoe");
                        goto InputsFinished;
                    }
                }
                else if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.WateringCan)
                {
                    bool watering = WaterGround(1);
                    if (watering)
                    {
                        StartToolAnim("UseWateringCan");
                        goto InputsFinished;
                    }
                }
                else if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Empty && actionsGameplay.FindAction("UseL").triggered)
                {
                    string pulling = PullTileContents(1);
                    if (pulling != null)
                    {
                        StartToolAnim(pulling);
                        goto InputsFinished;
                    }
                }
            }

            //item selection
            ItemButton = 0;
            if (actionsGameplay.FindAction("Item1").triggered && actionsGameplay.FindAction("Item1").ReadValue<float>() > 0) ItemButton = 1;
            else if (actionsGameplay.FindAction("Item2").triggered && actionsGameplay.FindAction("Item2").ReadValue<float>() > 0) ItemButton = 2;
            else if (actionsGameplay.FindAction("Item3").triggered && actionsGameplay.FindAction("Item3").ReadValue<float>() > 0) ItemButton = 3;
            else if (actionsGameplay.FindAction("Item4").triggered && actionsGameplay.FindAction("Item4").ReadValue<float>() > 0) ItemButton = 4;

            if (ItemButton != 0)
            {
                if (ItemButton == ActiveHotbarSlot) ActiveHotbarSlot = 0;
                else ActiveHotbarSlot = ItemButton;
                //enable this line once there's a change items animation (and when I've figured out playing upper-body animations on top of running)
                //animator.SetTrigger("ChangeItems");

                animator.SetBool("2H", Hotbar[ActiveHotbarSlot].TwoHanded);

                if (HeldItems[0]) Destroy(HeldItems[0]);
                if (HeldItems[1]) Destroy(HeldItems[1]);
                Transform ActiveHand;
                GameObject ActiveItem;
                if (Hotbar[ActiveHotbarSlot].TwoHanded)
                {
                    GameObject newOb = Hotbar[ActiveHotbarSlot].gameObject;
                    if (newOb)HeldItems[0] = Instantiate(newOb);
                    ActiveHand = LeftHand.transform;
                    ActiveItem = HeldItems[0];
                }
                else
                {
                    GameObject newOb = Hotbar[ActiveHotbarSlot].gameObject;
                    if (newOb) HeldItems[1] = Instantiate(newOb);
                    ActiveHand = RightHand.transform;
                    ActiveItem = HeldItems[1];
                }
                if (ActiveItem)
                {
                    ActiveItem.transform.SetParent(ActiveHand);
                    ActiveItem.transform.localPosition = Vector3.zero;
                    ActiveItem.transform.localRotation = Quaternion.identity;
                }

            }

            Movement = actionsGameplay.FindAction("Move").ReadValue<Vector2>();

            //experimenting with using a gamma curve for the stick inputs, to make slow walking easier
            float newMoveSpeed = Mathf.Pow(Movement.magnitude, 2.2f);
            Movement = Movement.normalized * newMoveSpeed;

            animator.SetFloat("NormalizedSpeed", Movement.magnitude);
            Movement *= RunSpeed;
            if (Movement.magnitude > .0001 && followCam.enabled)
            {
                //Debug.Log($"{Movement[0]}, {Movement[1]}");
                transform.rotation = Quaternion.LookRotation(new Vector3(Movement[0], 0, Movement[1]), Vector3.up);
                transform.Rotate(new Vector3(0, followCam.TwistAngle, 0));
            }
            animator.SetFloat("Speed", Movement.magnitude);

            float InteractDistance = InteractDistanceNear;
            if (Movement.magnitude > 0) InteractDistance = InteractDistanceFar;
            Vector3 InteractPoint = transform.position + transform.forward * InteractDistance;
            farmPlot.TargetTile(InteractPoint, Hotbar[ActiveHotbarSlot].toolType);
            InputsFinished:;
        }


    }

    public void AllowNextInput(int input)
    {
        //animation events can't send a bool, so we'll have to treat an int as one
        QueueNextInput = input != 0;

        //check for held inputs (for repeating actions, like hoeing multiple pieces of ground)
        if (QueueNextInput)
        {

        }
    }
    public void SetIFrames(int input)
    {
        iFrames = input != 0;
    }

    public bool HoeGround(int test)
    {
        float InteractDistance = InteractDistanceNear;
        if (Movement.magnitude > 0) InteractDistance = InteractDistanceFar;
        Vector3 InteractPoint = transform.position + transform.forward * InteractDistance;
        return farmPlot.HoeTile(InteractPoint, test != 0);
    }
    public bool WaterGround(int test)
    {
        float InteractDistance = InteractDistanceNear;
        if (Movement.magnitude > 0) InteractDistance = InteractDistanceFar;
        return farmPlot.WaterTile(transform.position + transform.forward * InteractDistance, test != 0);
    }
    public string PullTileContents(int test)
    {
        float InteractDistance = InteractDistanceNear;
        if (Movement.magnitude > 0) InteractDistance = InteractDistanceFar;
        return farmPlot.PullTileContents(transform.position + transform.forward * InteractDistance, test != 0);
    }
    void StartToolAnim(string AnimName)
    {
        //animator.SetTrigger($"Use{Hotbar[ActiveInventorySlot].toolType.ToString()}");
        animator.SetTrigger(AnimName);
        animator.SetFloat("Speed", 0);
        animator.SetFloat("NormalizedSpeed", 0);
        QueueNextInput = false;
    }
}

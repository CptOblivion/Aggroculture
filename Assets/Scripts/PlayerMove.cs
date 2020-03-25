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
    public float ToolDistanceFar = 2.1f;
    public float ToolDistanceDefault = .75f;
    public float GrabDistanceFar = 1;
    public float InteractDistance = 2;
    public FarmPlot farmPlot;
    public GameObject debugBuddy;
    public Transform SpinBone;
    public bool NoInteractionIfTooFar = false;
    public bool InteractFromTileCenter = true;
    public Transform FacingObject;

    public InventoryItem Tools_Empty;

    public static PlayerMove currentPlayer;

    GameObject[] HeldItems = { null, null };
    int ActiveHotbarSlot = 0;
    int ItemButton = 0;
    bool iFrames = false;
    bool QueueNextInput = true;//allow the next input to be queued while the current animation is running (typically enabled near the end of the animation)
    float ToolDistance = 0;
    float GrabDistance = 0;
    Vector2 LookInput;
    Vector2 LookVector;
    Vector2 MoveInput = Vector2.zero;
    Vector2 MoveVector;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Transform TargetToMatch;
    float MatchTargetFade = -1;
    float MatchTargetFadeTime;
    Vector3 MatchTargetOriginPos;
    Quaternion MatchTargetOriginRot;
    CharacterController characterController;
    InputActionMap actionsGameplay;
    PlayerInput playerInput;
    Quaternion SpinboneHome;
    [SerializeField]
    bool ReachTooFar = false;
    Vector2Int FromTile;
    Vector2Int ToTile;

    private void Awake()
    {
        currentPlayer = this;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        inputActions.Enable();
        actionsGameplay = inputActions.FindActionMap("Gameplay");
        debugBuddy = Instantiate(debugBuddy);
        debugBuddy.SetActive(false);
        playerInput = GetComponent<PlayerInput>();
        SpinboneHome = SpinBone.localRotation;
        FacingObject.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (QueueNextInput && !PauseManager.Paused)
        {
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
                    string InteractAnim = InteractTileContents(1);
                    if (InteractAnim != null)
                    {
                        StartToolAnim(InteractAnim);
                        goto InputsFinished;
                    }
                }
            }
            else if (actionsGameplay.FindAction("Interact").ReadValue<float>() > 0 && actionsGameplay.FindAction("Interact").triggered)
            {
                RaycastHit hit;
                if (Physics.Raycast(FacingObject.position,FacingObject.forward, out hit, InteractDistance))
                {
                    InteractTarget interactTarget = hit.transform.GetComponent<InteractTarget>();
                    if (interactTarget)
                    {
                        interactTarget.Interact();
                    }
                }
            }

            LookInput = actionsGameplay.FindAction("Look").ReadValue<Vector2>();
            MoveInput = actionsGameplay.FindAction("Move").ReadValue<Vector2>();

            MoveVector = GlobalTools.RotateVector2(MoveInput, followCam.TwistAngle);
            //experimenting with using a gamma curve for the stick inputs, to make slow walking easier
            //MoveInput = MoveInput.normalized * Mathf.Pow(MoveInput.magnitude, 2.2f);
            if (playerInput.currentControlScheme == "Keyboard")
            {
                Vector3 ScreenPosition = followCam.GetComponent<Camera>().WorldToScreenPoint(transform.position);
                LookVector = LookInput - new Vector2(ScreenPosition[0], ScreenPosition[1]);


                //ToolDistance = LookVector.magnitude / Screen.height * followCam.GetComponent<Camera>().orthographicSize * 2;
                RaycastHit rayHit;
                if (farmPlot.GetComponent<Collider>().Raycast(followCam.GetComponent<Camera>().ScreenPointToRay(new Vector3(LookInput[0], LookInput[1])), out rayHit, 100))
                {
                    if (InteractFromTileCenter)
                    {
                        FromTile = farmPlot.GlobalToTile(transform.position);
                        ToTile = farmPlot.GlobalToTile(rayHit.point);

                        //ToolDistance = (farmPlot.TileToGlobal(ToTile) - farmPlot.TileToGlobal(FromTile)).magnitude;
                        //if (ToolDistance > ToolDistanceFar * farmPlot.TileScale)
                        float TestDistance = ((Vector2)ToTile - (Vector2)FromTile).magnitude;
                        if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Empty) TestDistance += (ToolDistanceFar - GrabDistance);
                        if (TestDistance -.5f > ToolDistanceFar)
                        {
                            if (NoInteractionIfTooFar)
                            {
                                ReachTooFar = true;
                            }
                            else
                            {
                                Vector2 AnalogTile = ToTile - FromTile;
                                AnalogTile = AnalogTile.normalized * ToolDistanceFar;
                                ToTile = FromTile + new Vector2Int(Mathf.RoundToInt(AnalogTile.x), Mathf.RoundToInt(AnalogTile.y));
                            }
                        }
                        else ReachTooFar = false;
                    }
                    else
                    {
                        ToolDistance = (rayHit.point - transform.position).magnitude;
                        if (ToolDistance > ToolDistanceFar)
                        {
                            if (NoInteractionIfTooFar) ReachTooFar = true;
                            else ReachTooFar = false;
                            ToolDistance = ToolDistanceFar;
                        }
                        else ReachTooFar = false;
                    }
                    if (LookVector == Vector2.zero) LookVector = Vector2.up;


                }
                else
                {
                    ReachTooFar = true;
                    ToolDistance = ToolDistanceFar;
                }
                /*
                GrabDistance = ToolDistance;
                if (GrabDistance > GrabDistanceFar) GrabDistance = GrabDistanceFar;
                */
                GrabDistance = GrabDistanceFar;
            }
            else
            {
                ReachTooFar = false;
                GrabDistance = GrabDistanceFar;
                if (LookInput == Vector2.zero)
                {
                    ToolDistance = ToolDistanceDefault;
                    if (MoveVector != Vector2.zero) LookVector = MoveVector.normalized;
                }
                else
                {
                    LookVector = LookInput.normalized;
                    ToolDistance = ToolDistanceDefault + (ToolDistanceFar - ToolDistanceDefault) * LookInput.magnitude;
                }
            }

            if (followCam.enabled)
            {
                if (MoveInput == Vector2.zero)
                {
                    transform.rotation = Quaternion.LookRotation(new Vector3(LookVector[0], 0, LookVector[1]), Vector3.up);
                    transform.Rotate(new Vector3(0, followCam.TwistAngle, 0));
                    SpinBone.localRotation = SpinboneHome;
                    FacingObject.localRotation = Quaternion.identity;
                }
                else //if we're moving
                {
                    transform.rotation = Quaternion.LookRotation(new Vector3(MoveVector[0], 0, MoveVector[1]), Vector3.up);
                    transform.Rotate(new Vector3(0, followCam.TwistAngle, 0));
                    if (LookInput == Vector2.zero)
                    {
                        SpinBone.localRotation = SpinboneHome;
                        FacingObject.localRotation = Quaternion.identity;
                    }
                    else //if we're facing a direction (right stick pushed, or always on in mouse/keyboard mode)
                    {
                        SpinBone.rotation = Quaternion.LookRotation(new Vector3(LookVector[0], 0, LookVector[1]), Vector3.up);
                        SpinBone.Rotate(new Vector3(0, followCam.TwistAngle, 0));
                        FacingObject.rotation = SpinBone.rotation;
                        //FacingObject.rotation = Quaternion.LookRotation(new Vector3(LookVector[0], 0, LookVector[1]), Vector3.up);
                    }
                }

            }

            animator.SetFloat("NormalizedSpeed", MoveInput.magnitude);
            animator.SetBool("Moving", MoveInput.magnitude != 0);

            if (LookVector == Vector2.zero)
            {
                animator.SetFloat("SpeedX", 0);
                animator.SetFloat("SpeedY", MoveVector[1]);

            }
            else
            {
                animator.SetFloat("SpeedX", Vector2.Dot(LookVector.normalized, GlobalTools.RotateVector2(MoveVector, -90)));
                animator.SetFloat("SpeedY", Vector2.Dot(LookVector.normalized, MoveVector));
            }
            MoveVector *= RunSpeed;
            animator.SetFloat("Speed", MoveVector.magnitude);


            if (actionsGameplay.FindAction("Dodge").triggered && actionsGameplay.FindAction("Dodge").ReadValue<float>() > 0) 
            {
                if (MoveInput == Vector2.zero) animator.SetTrigger("DodgeHop");
                else animator.SetTrigger("DodgeRoll");
                AllowNextInput(0);
                iFrames = true;
                goto InputsFinished;
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
                //enable this line once (if) there's a "change items" animation (and when I've figured out playing upper-body animations on top of running)
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

            if (!ReachTooFar)
            {
                if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Empty)
                {
                    if (InteractFromTileCenter)
                    {
                        if (playerInput.currentControlScheme != "Keyboard")
                            ToTile = farmPlot.GlobalToTile(farmPlot.TileToGlobal(farmPlot.GlobalToTile(transform.position)) + FacingObject.forward * GrabDistance * farmPlot.TileScale);
                        farmPlot.TargetTile(ToTile, Hotbar[ActiveHotbarSlot].toolType);
                    }
                    else
                        farmPlot.TargetTile(transform.position + FacingObject.forward * GrabDistance, Hotbar[ActiveHotbarSlot].toolType);
                }
                else
                {
                    if (InteractFromTileCenter)
                    {
                        if (playerInput.currentControlScheme != "Keyboard")
                            ToTile = farmPlot.GlobalToTile(farmPlot.TileToGlobal(farmPlot.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * farmPlot.TileScale);
                        farmPlot.TargetTile(ToTile, Hotbar[ActiveHotbarSlot].toolType);
                    }
                    else
                        farmPlot.TargetTile(transform.position + FacingObject.forward * ToolDistance * farmPlot.TileScale, Hotbar[ActiveHotbarSlot].toolType);
                }
            }
            InputsFinished:;
        }
        if (MatchTargetFade > 0)
        {
            animator.applyRootMotion = false;
            if (MatchTargetFadeTime == 0)
                MatchTargetFade = 0;
            else
                MatchTargetFade -= Time.deltaTime / MatchTargetFadeTime;
            transform.position = Vector3.Lerp(MatchTargetOriginPos, TargetToMatch.position, 1 - MatchTargetFade);
            transform.rotation = Quaternion.Lerp(MatchTargetOriginRot, TargetToMatch.rotation, 1 - MatchTargetFade);
            if (MatchTargetFade <= 0)
            {
                TargetToMatch = null;
                animator.applyRootMotion = true;
            }
        }

    }
    public void AllowNextInput(int input)
    {
        QueueNextInput = input != 0;
    }
    public void SetIFrames(int input)
    {
        iFrames = input != 0;
    }

    public bool HoeGround(int test)
    {
        if (!ReachTooFar)
        {
            if (InteractFromTileCenter)
            {
                if (playerInput.currentControlScheme == "Keyboard")
                    return farmPlot.HoeTile(farmPlot.TileToGlobal(ToTile), test != 0);
                else
                    return farmPlot.HoeTile(farmPlot.TileToGlobal(farmPlot.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * farmPlot.TileScale, test != 0);
            }
            else
                return farmPlot.HoeTile(transform.position + FacingObject.forward * ToolDistance * farmPlot.TileScale, test != 0);
        }
        else return false;
    }
    public bool WaterGround(int test)
    {
        if (!ReachTooFar)
        {
            if (InteractFromTileCenter)
            {
                if (playerInput.currentControlScheme != "Keyboard")
                    ToTile = farmPlot.GlobalToTile(farmPlot.TileToGlobal(farmPlot.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * farmPlot.TileScale);
                return farmPlot.WaterTile(ToTile, test != 0);
            }
            else
                return farmPlot.WaterTile(transform.position + FacingObject.forward * ToolDistance * farmPlot.TileScale, test != 0);
        }
        else return false;
    }
    public string InteractTileContents(int test)
    {
        if (!ReachTooFar)
        {
            if (InteractFromTileCenter)
            {
                if (playerInput.currentControlScheme != "Keyboard")
                    ToTile = farmPlot.GlobalToTile(farmPlot.TileToGlobal(farmPlot.GlobalToTile(transform.position)) + FacingObject.forward * GrabDistance * farmPlot.TileScale);
                return farmPlot.PullTileContents(ToTile, test != 0);
            }
            else
                return farmPlot.PullTileContents(transform.position + FacingObject.forward * GrabDistance * farmPlot.TileScale, test != 0);
        }
        else return null;
    }
    void StartToolAnim(string AnimName)
    {
        //animator.SetTrigger($"Use{Hotbar[ActiveInventorySlot].toolType.ToString()}");
        animator.SetTrigger(AnimName);
        animator.SetFloat("Speed", 0);
        animator.SetFloat("NormalizedSpeed", 0);
        QueueNextInput = false;
    }

    public void MatchTarget(float MatchFade)
    {
        MatchTargetFade = 1;
        MatchTargetFadeTime = MatchFade;
        MatchTargetOriginPos = transform.position;
        MatchTargetOriginRot = transform.rotation;
    }

    public void Unequip()
    {
        ActiveHotbarSlot = 0;
        animator.SetBool("2H", false);
        if (HeldItems[0]) Destroy(HeldItems[0]);
        if (HeldItems[1]) Destroy(HeldItems[1]);
        HeldItems[0] = null;
        HeldItems[1] = null;
    }
}

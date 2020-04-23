using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChangeHotbar: UnityEvent<PlayerMain>
{
}

public class PlayerMain : CharacterBase
{
    public bool NoInteractionIfTooFar = true;
    public bool InteractFromTileCenter = true;

    //empty, hoe, watering can, empty, empty
    public InventoryItem[] Hotbar = new InventoryItem[5];
    [HideInInspector]
    public ChangeHotbar OnChangeHotbar = new ChangeHotbar();
    public GameObject LeftHand;
    public GameObject RightHand;
    public float ToolDistanceFar = 2.1f;
    public float ToolDistanceDefault = .75f;
    public float GrabDistanceFar = 1;
    public float InteractDistance = 5;

    public Transform SpinBone;

    public Transform FacingObject;
    public Transform Hat;

    //possible to have an initilization in InventoryItem that generates the data for an empty tool slot?
    public InventoryItem Tools_Empty;

    public static PlayerMain current;

    public float ExitCombatCooldown = .5f;
    float ExitCombatCooldownTimer = 0;
    public float DodgeRollCooldown = .25f;
    float DodgeRollCooldownTimer = 0;

    public float MaxTurnSpeed = 15f;
    public float MaxTurnSpeedAttack = 5f;
    Quaternion TurnTarget;
    Quaternion TurnOrigin;

    int LastSelectedWeapon = 1;

    FarmTileContents EquippedSeed = null;
    bool CanAct = true;
    GameObject[] HeldItems = { null, null };
    [HideInInspector]
    public int ActiveHotbarSlot = 0;
    int ItemButton = 0;
    float ToolDistance = 0;
    float GrabDistance = 0;
    Vector2 LookInput;
    Vector2 LookVector;
    Vector2 MoveInput = Vector2.zero;
    Vector2 MoveVector;
    [HideInInspector]
    public Transform TargetToMatch;
    float MatchTargetFade = -1;
    float MatchTargetFadeTime;
    Vector3 MatchTargetOriginPos;
    Quaternion MatchTargetOriginRot;
    InputActionMap actionsGameplay;
    [HideInInspector]
    public PlayerInput playerInput;
    Quaternion SpinboneHome;
    bool ReachTooFar = false;
    Vector2Int FromTile;
    Vector2Int ToTile;
    Transform DummyHat;
    bool ControlEnabledThisTurn = true;
    int LastAttackState = 0;
    float LastAttackStateTime;

    [HideInInspector]
    public InputAction inputMove;
    [HideInInspector]
    public InputAction inputLook;
    [HideInInspector]
    public InputAction inputPause;
    [HideInInspector]
    public InputAction inputInteract;
    [HideInInspector]
    public InputAction inputDodge;
    [HideInInspector]
    public InputAction inputUseL;
    [HideInInspector]
    public InputAction inputUseR;
    [HideInInspector]
    public Vector2 inputLastMove;
    [HideInInspector]
    public Vector2 inputLastLook;


    protected override void Awake()
    {
        base.Awake();
        current = this;

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Enable();
        actionsGameplay = playerInput.actions.FindActionMap("Gameplay");
        SpinboneHome = SpinBone.localRotation;
        FacingObject.localRotation = Quaternion.identity;

        inputMove = actionsGameplay.FindAction("Move");
        inputLook = actionsGameplay.FindAction("Look");
        inputPause = actionsGameplay.FindAction("Pause");
        inputInteract = actionsGameplay.FindAction("Interact");
        inputDodge = actionsGameplay.FindAction("Dodge");
        inputUseL = actionsGameplay.FindAction("UseL");
        inputUseR = actionsGameplay.FindAction("UseR");
        inputLastMove = inputMove.ReadValue<Vector2>();
        inputLastLook = inputLook.ReadValue<Vector2>();
        StartingLayer = gameObject.layer = (int)GlobalTools.Layers.Player; //just making sure
        CanMove = CanTurn = true;
    }

    private void LateUpdate()
    {
        //store current values for comparison next frame
        inputLastMove = inputMove.ReadValue<Vector2>();
        inputLastLook = inputLook.ReadValue<Vector2>();
    }
    protected override void Update()
    {
        base.Update();
        if (!PauseManager.Paused)
        {
            if (InCombat && ExitCombatCooldownTimer > 0)
            {
                ExitCombatCooldownTimer -= Time.deltaTime;
                if (ExitCombatCooldownTimer <= 0)
                {
                    InCombat = false;
                    animator.SetFloat("InCombat", 0);
                    //auto switch back to last-equipped item? Probably a bad idea
                }
            }
            if (alive)
            {
                if (CanAct)
                {
                    if ((inputUseR.triggered && inputUseR.ReadValue<float>() > 0) || (InCombat && inputUseL.triggered && inputUseL.ReadValue<float>() > 0))
                    {
                        //if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Hoe || Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.trowel)
                        if(CheckTagInItem("Weapon"))
                        {
                            SoftForceAnimator("Attack");
                            animator.SetFloat("Speed", 0);
                            transform.rotation = SpinBone.rotation;
                            SpinBone.localRotation = SpinboneHome;
                            AllowNextInput(false);
                        }
                    }
                }
                if (CanMove || CanTurn)
                {
                    if (CanMove)
                    {
                        if (inputUseL.ReadValue<float>() > 0 && !InCombat)
                        {
                            string ToolType = null;

                            for (int i = 0; i < Hotbar[ActiveHotbarSlot].Tags.Length; i++)
                            {
                                if (Hotbar[ActiveHotbarSlot].Tags[i] == "Hoe")
                                {
                                    ToolType = "Hoe";
                                    break;
                                }
                                else if (Hotbar[ActiveHotbarSlot].Tags[i] == "WateringCan")
                                {
                                    ToolType = "WateringCan";
                                    break;
                                }
                            }
                            if (ToolType == "Hoe")
                            {
                                bool hoeing = HoeGround(1);
                                if (hoeing)
                                {
                                    StartToolAnim("UseHoe");
                                    goto InputsFinished;
                                }
                            }
                            else if (ToolType == "WateringCan")
                            {
                                bool watering = WaterGround(1);
                                if (watering)
                                {
                                    StartToolAnim("UseWateringCan");
                                    goto InputsFinished;
                                }
                            }
                            else if (ToolType == null && inputUseL.triggered)
                            {
                                string InteractAnim = InteractTileContents(1);
                                if (InteractAnim != null)
                                {
                                    StartToolAnim(InteractAnim);
                                    goto InputsFinished;
                                }
                            }
                        }
                        else if (Physics.Raycast(FacingObject.position, FacingObject.forward, out RaycastHit hit, InteractDistance))
                        {
                            InteractTarget interactTarget = hit.transform.GetComponent<InteractTarget>();
                            if (interactTarget)
                            {
                                if (inputInteract.ReadValue<float>() > 0 && inputInteract.triggered)
                                    interactTarget.Interact();
                                else
                                    interactTarget.Highlight();
                            }
                        }
                    }

                    LookInput = inputLook.ReadValue<Vector2>();
                    MoveInput = inputMove.ReadValue<Vector2>();

                    MoveVector = GlobalTools.RotateVector2(MoveInput, FollowCam.current.TwistAngle);

                    //experimenting with using a gamma curve for the stick inputs, to make slow walking easier
                    //MoveInput = MoveInput.normalized * Mathf.Pow(MoveInput.magnitude, 2.2f);

                    if (playerInput.currentControlScheme == "Keyboard")
                    {
                        Vector3 ScreenPosition = FollowCam.current.cam.WorldToScreenPoint(transform.position);
                        LookVector = LookInput - new Vector2(ScreenPosition[0], ScreenPosition[1]);

                        if (FarmPlot.current.GetComponent<Collider>().Raycast(FollowCam.current.cam.ScreenPointToRay(new Vector3(LookInput[0], LookInput[1])), out RaycastHit hit, 100))
                        {
                            if (InteractFromTileCenter)
                            {
                                FromTile = FarmPlot.current.GlobalToTile(transform.position);
                                ToTile = FarmPlot.current.GlobalToTile(hit.point);

                                float TestDistance = ((Vector2)ToTile - (Vector2)FromTile).magnitude;
                                if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Empty) TestDistance += (ToolDistanceFar - GrabDistance);
                                if (TestDistance - .5f > ToolDistanceFar)
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
                                ToolDistance = (hit.point - transform.position).magnitude;
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
                        GrabDistance = GrabDistanceFar;
                    }
                    else
                    {
                        ReachTooFar = false;
                        GrabDistance = GrabDistanceFar;
                        if (LookInput == Vector2.zero)
                        {
                            ToolDistance = ToolDistanceDefault;
                            if (MoveVector != Vector2.zero)
                                LookVector = MoveVector.normalized;
                        }
                        else
                        {
                            LookVector = LookInput.normalized;
                            ToolDistance = ToolDistanceDefault + (ToolDistanceFar - ToolDistanceDefault) * LookInput.magnitude;
                        }
                    }

                    if (FollowCam.current.enabled)
                    {
                        TurnOrigin = SpinBone.rotation;
                        if (MoveInput == Vector2.zero)
                        {
                            if (LookInput != Vector2.zero)
                            {
                                transform.rotation = Quaternion.LookRotation(new Vector3(LookVector[0], 0, LookVector[1]), Vector3.up);
                                transform.Rotate(new Vector3(0, FollowCam.current.TwistAngle, 0));
                            }
                            TurnTarget = transform.rotation;
                        }
                        else //if we're moving
                        {
                            transform.rotation = Quaternion.LookRotation(new Vector3(MoveVector[0], 0, MoveVector[1]), Vector3.up);
                            transform.Rotate(new Vector3(0, FollowCam.current.TwistAngle, 0));
                            if (LookInput == Vector2.zero)
                                TurnTarget = transform.rotation;
                            else //if we're facing a direction (right stick pushed, or always on in mouse/keyboard mode)
                                TurnTarget = Quaternion.LookRotation(new Vector3(LookVector[0], 0, LookVector[1]), Vector3.up);
                        }
                        if (CanMove)
                        {
                            SpinBone.rotation = Quaternion.RotateTowards(TurnOrigin, TurnTarget, MaxTurnSpeed * Time.deltaTime * 60f);
                        }
                        else
                        {
                            transform.rotation = Quaternion.RotateTowards(TurnOrigin, TurnTarget, MaxTurnSpeedAttack * Time.deltaTime * 60f);
                            SpinBone.rotation = transform.rotation;
                        }
                        FacingObject.rotation = SpinBone.rotation;

                    }

                    if (CanMove)
                    {
                        animator.SetFloat("NormalizedSpeed", MoveInput.magnitude);
                        animator.SetBool("Moving", MoveInput.magnitude != 0);

                        if (LookVector == Vector2.zero)
                        {
                            animator.SetFloat("SpeedX", 0);
                            animator.SetFloat("SpeedY", MoveVector[1]);

                        }
                        else
                        {
                            animator.SetFloat("SpeedX", Vector3.Dot(transform.forward, -SpinBone.right) * MoveInput.magnitude);
                            animator.SetFloat("SpeedY", Vector3.Dot(transform.forward, SpinBone.forward) * MoveInput.magnitude);
                        }
                        MoveVector *= RunSpeed;
                        animator.SetFloat("Speed", MoveVector.magnitude);

                        if (actionsGameplay.FindAction("Dodge").triggered && actionsGameplay.FindAction("Dodge").ReadValue<float>() > 0)
                        {
                            if (MoveInput != Vector2.zero)
                                animator.SetBool("Moving", true);
                            SoftForceAnimator("Dodge");
                            SetIFrames(1);
                            AllowNextInput(false);
                            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                            {
                                LastAttackState = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                                LastAttackStateTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                            }
                            goto InputsFinished;
                        }
                    }
                    else
                    {
                        animator.SetBool("Moving", false);
                        animator.SetFloat("NormalizedSpeed", 0);
                        animator.SetFloat("Speed", 0);
                    }



                    //item selection
                    ItemButton = 0;
                    if (actionsGameplay.FindAction("Item1").triggered && actionsGameplay.FindAction("Item1").ReadValue<float>() > 0) ItemButton = 1;
                    else if (actionsGameplay.FindAction("Item2").triggered && actionsGameplay.FindAction("Item2").ReadValue<float>() > 0) ItemButton = 2;
                    else if (actionsGameplay.FindAction("Item3").triggered && actionsGameplay.FindAction("Item3").ReadValue<float>() > 0) ItemButton = 3;
                    else if (actionsGameplay.FindAction("Item4").triggered && actionsGameplay.FindAction("Item4").ReadValue<float>() > 0) ItemButton = 4;

                    if (ItemButton != 0)
                    {
                        if (ItemButton == ActiveHotbarSlot) ChangeHotbarSlot(0);
                        else ChangeHotbarSlot(ItemButton);
                    }

                    //indicate to player if they can interact with the targeted tile with the current tool:
                    if (!ReachTooFar && !InCombat)
                    {
                        if (Hotbar[ActiveHotbarSlot].toolType == InventoryItem.ToolTypes.Empty)
                        {
                            if (InteractFromTileCenter)
                            {
                                if (playerInput.currentControlScheme != "Keyboard")
                                    ToTile = FarmPlot.current.GlobalToTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * GrabDistance * FarmPlot.current.TileScale);
                                FarmPlot.current.TargetTile(ToTile, Hotbar[ActiveHotbarSlot].toolType);
                            }
                            else
                                FarmPlot.current.TargetTile(transform.position + FacingObject.forward * GrabDistance, Hotbar[ActiveHotbarSlot].toolType);
                        }
                        else
                        {
                            if (InteractFromTileCenter)
                            {
                                if (playerInput.currentControlScheme != "Keyboard")
                                    ToTile = FarmPlot.current.GlobalToTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale);
                                FarmPlot.current.TargetTile(ToTile, Hotbar[ActiveHotbarSlot].toolType);
                            }
                            else
                                FarmPlot.current.TargetTile(transform.position + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale, Hotbar[ActiveHotbarSlot].toolType);
                        }
                    }
                    InputsFinished:;
                }
            }
            else
            {

            }
            if (ControlEnabledThisTurn)
                ControlEnabledThisTurn = false;
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

    private void OnDestroy()
    {
        if (current == this)
            current = null;
    }

    bool CheckTagInItem(string tag, int InventorySlot = -1)
    {
        if (InventorySlot == -1)
            InventorySlot = ActiveHotbarSlot;
        for (int i = 0; i < Hotbar[InventorySlot].Tags.Length; i++)
        {
            if (Hotbar[InventorySlot].Tags[i]== tag)
            {
                return true;
            }
        }
        return false;
    }

    protected override void AddToTargetedBy(CharacterBase targeter)
    {
        base.AddToTargetedBy(targeter);

        //equip a weapon if we weren't targeted by anything before:
        if (TargetedBy.Count == 1)
        {
            InCombat = true;
            ExitCombatCooldownTimer = -1;
            ChangeHotbarSlot(LastSelectedWeapon);
            animator.SetFloat("InCombat", 1);
        }
    }

    protected override void RemoveFromTargetedBy(CharacterBase targeter)
    {
        base.RemoveFromTargetedBy(targeter);

        
        if (InCombat == false)
        {
            //we're staying in combat mode for a bit to let button mashing finish
            InCombat = true;
            ExitCombatCooldownTimer = ExitCombatCooldown;
        }
    }

    //public delegate void ChangeHotbar();
    //public ChangeHotbar HotbarChanged;

    void ChangeHotbarSlot(int SlotNumber)
    {
        if (ActiveHotbarSlot != SlotNumber)
        {
            if (CheckTagInItem("Weapon"))
                LastSelectedWeapon = ActiveHotbarSlot;
            ActiveHotbarSlot = SlotNumber;
            //enable this line once (if) there's a "change items" animation (and when I've figured out playing upper-body animations on top of running)
            //animator.SetTrigger("ChangeItems");

            animator.SetBool("2H", Hotbar[SlotNumber].TwoHanded);

            if (HeldItems[0]) Destroy(HeldItems[0]);
            if (HeldItems[1]) Destroy(HeldItems[1]);
            Transform ActiveHand;
            GameObject ActiveItem;
            if (Hotbar[SlotNumber].TwoHanded)
            {
                GameObject newOb = Hotbar[SlotNumber].gameObject;
                if (newOb) HeldItems[0] = Instantiate(newOb);
                ActiveHand = LeftHand.transform;
                ActiveItem = HeldItems[0];
            }
            else
            {
                GameObject newOb = Hotbar[SlotNumber].gameObject;
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
            //HotbarChanged.Invoke();
            OnChangeHotbar.Invoke(this);
        }
    }

    string[] TriggerList = new string[] { "Attack", "Dodge"};

    void SoftForceAnimator(string trigger = "")
    {
        /*
         * Unsets all triggers in Triggerlist, except the one passed in the argument (EG to make sure an attack and a dodge input don't stack up, or to clear everything on taking damage)
         */
        if (trigger != "")
            animator.SetTrigger(trigger);
        for (int i = 0; i < TriggerList.Length; i++)
        {
            if (TriggerList[i] != trigger)
                animator.ResetTrigger(TriggerList[i]);
        }
    }

    public override void ApplyDamage(DamageReceiver receiver)
    {
        base.ApplyDamage(receiver);

        if (DamageFadeTimer > 0) //there's probably a better way to figure out if a flinch/slide/knockdown was triggered in base.ApplyDamage
        {
            SoftForceAnimator();
            SpinBone.localRotation = SpinboneHome;
            CanAct = false;
        }
    }
    public override void Kill()
    {
        CurrentHealth = MaxHealth;
        PauseManager.Pause();
    }

    //might not be necessary
    private void OnAnimatorMove()
    {
        if (animator.applyRootMotion)
        {
            characterController.Move(animator.rootPosition - transform.position + SnapToGround());
        }
    }
    void AllowNextInput(bool Move, bool Attack = false, bool Turn = false)
    {
        if (!CanMove && Move)
            ControlEnabledThisTurn = true;
        CanMove = Move;
        CanAct = Attack;
        CanTurn = Turn;
        //CanAct = CanMove = CanTurn =  Move;
        //if (Attack) CanAct = true;
        //if (Turn) CanTurn = true;
    }
    public void InputDisable()
    {
        AllowNextInput(false);
    }
    public void InputAllow()
    {
        bool NextMoveAlreadyQueued = false;
        foreach (string trigger in TriggerList)
        {
            if (animator.GetBool(trigger))
            {
                NextMoveAlreadyQueued = true;
                break;
            }
        }
        if (!NextMoveAlreadyQueued)
        {
            AllowNextInput(true, true, true);
        }

    }
    public void InputAttackOnly(int Turn)
    {
        AttackReleaseReset();
        AllowNextInput(false, true, Turn != 0);
    }
    public void AttackRelease()
    {
        animator.SetTrigger("AttackRelease");
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Dodge") && LastAttackState != 0 && animator.GetBool("Attack"))
        {
            animator.Play(LastAttackState, 0, LastAttackStateTime);
            LastAttackState = 0;
            AttackRelease();
        }
        LastAttackState = 0;
    }
    public void AttackReleaseReset()
    {
        animator.ResetTrigger("AttackRelease");
    }

    public bool HoeGround(int test)
    {
        if (!ReachTooFar)
        {
            if (InteractFromTileCenter)
            {
                if (playerInput.currentControlScheme == "Keyboard")
                    return FarmPlot.current.HoeTile(FarmPlot.current.TileToGlobal(ToTile), test != 0);
                else
                    return FarmPlot.current.HoeTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale, test != 0);
            }
            else
                return FarmPlot.current.HoeTile(transform.position + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale, test != 0);
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
                    ToTile = FarmPlot.current.GlobalToTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale);
                return FarmPlot.current.WaterTile(ToTile, test != 0);
            }
            else
                return FarmPlot.current.WaterTile(transform.position + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale, test != 0);
        }
        else return false;
    }

    public bool PlantSeed(int test)
    {
        if (!ReachTooFar)
        {
            if (InteractFromTileCenter)
            {
                if (playerInput.currentControlScheme != "Keyboard")
                    ToTile = FarmPlot.current.GlobalToTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * GrabDistance * FarmPlot.current.TileScale);
                return FarmPlot.current.PlantTile(ToTile, EquippedSeed, test != 0);
            }
            else
                return FarmPlot.current.PlantTile(transform.position + FacingObject.forward * ToolDistance * FarmPlot.current.TileScale, EquippedSeed, test != 0);
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
                    ToTile = FarmPlot.current.GlobalToTile(FarmPlot.current.TileToGlobal(FarmPlot.current.GlobalToTile(transform.position)) + FacingObject.forward * GrabDistance * FarmPlot.current.TileScale);
                return FarmPlot.current.PullTileContents(ToTile, test != 0);
            }
            else
                return FarmPlot.current.PullTileContents(transform.position + FacingObject.forward * GrabDistance * FarmPlot.current.TileScale, test != 0);
        }
        else return null;
    }
    void StartToolAnim(string AnimName)
    {
        //animator.SetTrigger($"Use{Hotbar[ActiveInventorySlot].toolType.ToString()}");
        animator.SetTrigger(AnimName);
        animator.SetFloat("Speed", 0);
        animator.SetFloat("NormalizedSpeed", 0);
        AllowNextInput(false);
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

    public void HangHat(int toggle)
    {
        if (toggle == 0) {
            if (DummyHat)
            {
                Destroy(DummyHat.gameObject);
                DummyHat = null;
            }
            Hat.gameObject.SetActive(true);
        }
        else
        {
            DummyHat = Instantiate(Hat);
            DummyHat.position = Hat.position;
            DummyHat.rotation = Hat.rotation;
            DummyHat.localScale = Hat.lossyScale;
            Hat.gameObject.SetActive(false);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(PlayerMain))]
public class PlayerMainInspector : CharacterBaseInspector
{
    static bool ShowCharacterBase = true;
    static bool ShowSetupPlayer = false;

    SerializedProperty LeftHand;
    SerializedProperty RightHand;
    SerializedProperty SpinBone;
    SerializedProperty FacingObject;
    SerializedProperty Hat;
    SerializedProperty Tools_Empty;

    SerializedProperty NoInteractionIfTooFar;
    SerializedProperty InteractFromTileCenter;
    SerializedProperty Hotbar;
    SerializedProperty ToolDistanceFar;
    SerializedProperty GrabDistanceFar;
    SerializedProperty InteractDistance;
    SerializedProperty ExitCombatCooldown;
    SerializedProperty DodgeRollCooldown;
    SerializedProperty MaxTurnSpeed;
    SerializedProperty MaxTurnSpeedAttack;


    protected override void OnEnable()
    {
        base.OnEnable();
        NoInteractionIfTooFar = serializedObject.FindProperty("NoInteractionIfTooFar");
        InteractFromTileCenter = serializedObject.FindProperty("InteractFromTileCenter");
        Hotbar = serializedObject.FindProperty("Hotbar");
        LeftHand = serializedObject.FindProperty("LeftHand");
        RightHand = serializedObject.FindProperty("RightHand");
        ToolDistanceFar = serializedObject.FindProperty("ToolDistanceFar");
        GrabDistanceFar = serializedObject.FindProperty("GrabDistanceFar");
        InteractDistance = serializedObject.FindProperty("InteractDistance");
        SpinBone = serializedObject.FindProperty("SpinBone");
        FacingObject = serializedObject.FindProperty("FacingObject");
        Hat = serializedObject.FindProperty("Hat");
        Tools_Empty = serializedObject.FindProperty("Tools_Empty");
        ExitCombatCooldown = serializedObject.FindProperty("ExitCombatCooldown");
        DodgeRollCooldown = serializedObject.FindProperty("DodgeRollCooldown");
        MaxTurnSpeed = serializedObject.FindProperty("MaxTurnSpeed");
        MaxTurnSpeedAttack = serializedObject.FindProperty("MaxTurnSpeedAttack");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;
        ShowCharacterBase = EditorGUILayout.Foldout(ShowCharacterBase, "Character Base Properties", true);
        if (ShowCharacterBase)
        {
            base.OnInspectorGUI();
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical("box");
        EditorGUI.indentLevel++;
        ShowSetupPlayer = EditorGUILayout.Foldout(ShowSetupPlayer, "Player Setup", true);
        if (ShowSetupPlayer)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.PropertyField(LeftHand);
            EditorGUILayout.PropertyField(RightHand);
            EditorGUILayout.PropertyField(SpinBone);
            EditorGUILayout.PropertyField(FacingObject);
            EditorGUILayout.PropertyField(Hat);
            EditorGUILayout.PropertyField(Tools_Empty);

            EditorGUILayout.EndVertical();
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();


        EditorGUILayout.PropertyField(NoInteractionIfTooFar);
        EditorGUILayout.PropertyField(InteractFromTileCenter);
        EditorGUILayout.PropertyField(Hotbar);
        EditorGUILayout.PropertyField(ToolDistanceFar);
        EditorGUILayout.PropertyField(GrabDistanceFar);
        EditorGUILayout.PropertyField(InteractDistance);
        EditorGUILayout.PropertyField(ExitCombatCooldown);
        EditorGUILayout.PropertyField(DodgeRollCooldown);
        EditorGUILayout.PropertyField(MaxTurnSpeed);
        EditorGUILayout.PropertyField(MaxTurnSpeedAttack);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

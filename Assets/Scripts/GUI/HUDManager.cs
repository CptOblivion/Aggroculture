﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]
public class HUDManager : MonoBehaviour
{
    public RawImage DrawGame;
    [Range(.01f,1f)]
    public float RenderScale = 0.5f;
    float LastRenderScale = .5f;
    public Transform Healthbar;
    public static float ScaleDifference;
    public static float ActualRenderScale;
    public static UnityEvent OnUpdateScreenShape;

    public static HUDManager current;

    public Color HotbarSelected = Color.white;
    public Color HotbarUnselected = Color.gray;
    public RawImage[] IconDpad = new RawImage[4];
    public RawImage[] IconRow = new RawImage[4];

    public float TargetWidth = 1920;
    public float TargetHeight = 1080;

    public Text SeasonTitleText;
    public Text WeekdayTitleText;

    float LastWindowWidth;
    float LastWindowHeight;

    int Init = 2;

    Animator animator;
    void Start()
    {
        current = this;
        OnUpdateScreenShape = new UnityEvent();
        ActualRenderScale = RenderScale;
        Camera.main.targetTexture = new RenderTexture((int)(Screen.width * ActualRenderScale), (int)(Screen.height * ActualRenderScale),0);
        DrawGame.texture = Camera.main.targetTexture;
        DrawGame.rectTransform.localScale = new Vector3(Screen.width, Screen.height, 1);
        animator = GetComponent<Animator>();
        PlayerMain.current.OnChangeHotbar.AddListener(UpdateHotbar);
        animator.SetBool("Dpad", PlayerMain.current.playerInput.currentControlScheme != "Keyboard");
        LastWindowHeight = Screen.height;
        LastWindowWidth = Screen.width;
        LastRenderScale = RenderScale;
        DrawGame.gameObject.SetActive(true);
        UpdateElementPositions();
        UpdateHotbar(PlayerMain.current);
        TimeManager.UpdateDay(true);
        //SeasonTitleText.gameObject.SetActive(false);
        //WeekdayTitleText.gameObject.SetActive(false);

        if (PlayerMain.current)
        {
            PlayerMain.current.playerInput.controlsChangedEvent.AddListener(ChangeControlSchemeDisplay);
            Cursor.visible = PlayerMain.current.playerInput.currentControlScheme == "Keyboard";
        }
        else
            Debug.Log("No current player!");
    }

    void Update()
    {
        if (Init > 0)
            Init--;
        else if (Init == 0)
        {
            UpdateElementPositions();
            Init--;
        }
        if (RenderScale != LastRenderScale)
        {
            ActualRenderScale = RenderScale;
        }
        if (Screen.width != LastWindowWidth || Screen.height != LastWindowHeight || ActualRenderScale != LastRenderScale)
        {
            UpdateElementPositions();
            LastWindowHeight = Screen.height;
            LastWindowWidth = Screen.width;
            LastRenderScale = ActualRenderScale;
            if (Camera.main.targetTexture)
            {
                Camera.main.targetTexture.Release();
                Camera.main.targetTexture = new RenderTexture((int)(Screen.width * ActualRenderScale), (int)(Screen.height * ActualRenderScale),0);
                DrawGame.texture = Camera.main.targetTexture;
                DrawGame.rectTransform.localScale = new Vector3(Screen.width, Screen.height, 1);
            }
        }
        Healthbar.localScale = new Vector3(PlayerMain.current.CurrentHealth / PlayerMain.current.MaxHealth, 1, 1);
    }

    void ChangeControlSchemeDisplay(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        animator.SetBool("Dpad", playerInput.currentControlScheme != "Keyboard");
        Cursor.visible = playerInput.currentControlScheme == "Keyboard";
        if (EventSystem.current.currentSelectedGameObject)
        {
            if (playerInput.currentControlScheme == "Keyboard")
            {
                //TODO: move to PauseManager?
                //and then stop Using UnityEngine.EventSystems in this script
                //also, our playerInput should be universally referencable, not attached to the player object that doesn't exist in the main menu (unless some sub-version of it could?)
                EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().OnDeselect(null);
            }
            else
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().OnSelect(null);
            }
        }
    }

    void UpdateElementPositions()
    {
        ScaleDifference = Mathf.Sqrt((Screen.width / 1920f) * (Screen.height / 1080f)) * ActualRenderScale;
        OnUpdateScreenShape.Invoke();
    }

    void UpdateHotbar(PlayerMain playerMain)
    {
        for (int i = 0; i < 4; i++)
        {
            InventorySlot slot = PlayerInventory.GetHotbarEntry(i);
            if (slot != null)
            {
                IconDpad[i].texture = slot.item.Icon;
                IconRow[i].texture = slot.item.Icon;
                if (playerMain.ActiveHotbarSlot == i)
                {
                    IconDpad[i].color = HotbarSelected;
                    IconRow[i].color = HotbarSelected;
                }
                else
                {
                    IconDpad[i].color = HotbarUnselected;
                    IconRow[i].color = HotbarUnselected;
                }
            }
        }
    }
}

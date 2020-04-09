using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class HUDManager : MonoBehaviour
{
    public Transform Healthbar;
    public Transform HealthbarFrame;
    float HealthbarFrameSize;
    public Transform HotbarFrame;
    float HotbarFrameSize;

    public Color HotbarSelected = Color.white;
    public Color HotbarUnselected = Color.gray;
    public RawImage[] IconDpad = new RawImage[4];
    public RawImage[] IconRow = new RawImage[4];

    public float TargetWidth = 1920;
    public float TargetHeight = 1080;

    float LastWindowWidth;
    float LastWindowHeight;

    Animator animator;
    void Start()
    {
        HealthbarFrameSize = HealthbarFrame.localScale.x;
        HotbarFrameSize = HotbarFrame.localScale.x;
        animator = GetComponent<Animator>();
        PlayerMain.current.OnChangeHotbar.AddListener(UpdateHotbar);
        animator.SetBool("Dpad", PlayerMain.current.playerInput.currentControlScheme != "Keyboard");
        LastWindowHeight = Screen.height;
        LastWindowWidth = Screen.width;
        UpdateElementPositions();
        UpdateHotbar(PlayerMain.current);

        if (PlayerMain.current)
        {
            PlayerMain.current.playerInput.controlsChangedEvent.AddListener(ChangeControlScheme);
            Cursor.visible = PlayerMain.current.playerInput.currentControlScheme == "Keyboard";
        }
        else
            Debug.Log("No current player!");
    }

    void Update()
    {
        if (Screen.width != LastWindowWidth || Screen.height != LastWindowHeight)
        {
            UpdateElementPositions();
            LastWindowHeight = Screen.height;
            LastWindowWidth = Screen.width;
        }
        Healthbar.localScale = new Vector3(PlayerMain.current.CurrentHealth / PlayerMain.current.MaxHealth, 1, 1);
    }

    void ChangeControlScheme(UnityEngine.InputSystem.PlayerInput playerInput)
    {
        animator.SetBool("Dpad", playerInput.currentControlScheme != "Keyboard");
        Cursor.visible = playerInput.currentControlScheme == "Keyboard";
    }

    void UpdateElementPositions()
    {
        float ScaleDifference = Mathf.Sqrt((Screen.width / 1920f) * (Screen.height / 1080f));
        HotbarFrame.localPosition = new Vector3(-Screen.width / 2, -Screen.height / 2, 0);
        HotbarFrame.localScale = new Vector3(ScaleDifference, ScaleDifference, ScaleDifference) * HotbarFrameSize;
        HealthbarFrame.localPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        HealthbarFrame.localScale = new Vector3(ScaleDifference, ScaleDifference, ScaleDifference) * HealthbarFrameSize;


    }

    void UpdateHotbar(PlayerMain playerMain)
    {
        for (int i = 0; i < 4; i++)
        {
            IconDpad[i].texture = playerMain.Hotbar[i+1].Icon;
            IconRow[i].texture = playerMain.Hotbar[i+1].Icon;
            if (playerMain.ActiveHotbarSlot == i+1)
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

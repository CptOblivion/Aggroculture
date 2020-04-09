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

    public RawImage IconDpad1;
    public RawImage IconDpad2;
    public RawImage IconDpad3;
    public RawImage IconDpad4;

    public RawImage IconRow1;
    public RawImage IconRow2;
    public RawImage IconRow3;
    public RawImage IconRow4;

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
        PlayerMain.current.ControlSchemeChanged.AddListener(ChangeControlScheme);
        animator.SetBool("Dpad", PlayerMain.current.LastControlScheme != "Keyboard");
        LastWindowHeight = Screen.height;
        LastWindowWidth = Screen.width;
        UpdateElementPositions();
        UpdateHotbar();

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

    void ChangeControlScheme()
    {
        //Debug.Log($"Changing control scheme to {PlayerMain.current.LastControlScheme}");
        animator.SetBool("Dpad", PlayerMain.current.LastControlScheme != "Keyboard");
    }

    void UpdateElementPositions()
    {
        float ScaleDifference = Mathf.Sqrt((Screen.width / 1920f) * (Screen.height / 1080f));
        HotbarFrame.localPosition = new Vector3(-Screen.width / 2, -Screen.height / 2, 0);
        HotbarFrame.localScale = new Vector3(ScaleDifference, ScaleDifference, ScaleDifference) * HotbarFrameSize;
        HealthbarFrame.localPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        HealthbarFrame.localScale = new Vector3(ScaleDifference, ScaleDifference, ScaleDifference) * HealthbarFrameSize;


    }

    void UpdateHotbar()
    {
        IconDpad1.texture = PlayerMain.current.Hotbar[1].Icon;
        IconDpad2.texture = PlayerMain.current.Hotbar[2].Icon;
        IconDpad3.texture = PlayerMain.current.Hotbar[3].Icon;
        IconDpad4.texture = PlayerMain.current.Hotbar[4].Icon;

        IconRow1.texture = PlayerMain.current.Hotbar[1].Icon;
        IconRow2.texture = PlayerMain.current.Hotbar[2].Icon;
        IconRow3.texture = PlayerMain.current.Hotbar[3].Icon;
        IconRow4.texture = PlayerMain.current.Hotbar[4].Icon;
    }
}

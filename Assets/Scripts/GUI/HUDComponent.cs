using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDComponent : MonoBehaviour
{
    public bool ScaleOnly = false;
    [Range(-1, 1)]
    public float PositionX = 0;
    [Range(-1, 1)]
    public float PositionY = 0;
    float Scale = 1;

    public bool ResolutionIndependent = false;

    int Init = 0;
    void Awake()
    {
        Scale = transform.localScale.x;
    }

    private void Update()
    {
        if (Init == 0)
            Init++;
        else
        {
            HUDManager.OnUpdateScreenShape.AddListener(UpdatePosition);
            enabled = false;
        }
    }
    private void OnEnable()
    {
        UpdatePosition();
    }
    private void OnDestroy()
    {
        HUDManager.OnUpdateScreenShape.RemoveListener(UpdatePosition);
    }

    public void UpdatePosition()
    {
        if (ResolutionIndependent && HUDManager.ActualRenderScale != 0)
        {
            if (!ScaleOnly) transform.localPosition = new Vector3(Screen.width / 2 * PositionX, Screen.height / 2 * PositionY, 0);
            transform.localScale = Vector3.one * Scale * HUDManager.ScaleDifference / HUDManager.ActualRenderScale;

        }
        else
        {
            if (!ScaleOnly) transform.localPosition = new Vector3(Screen.width / 2 * HUDManager.ActualRenderScale * PositionX, Screen.height / 2 * HUDManager.ActualRenderScale * PositionY, 0);
            transform.localScale = Vector3.one * Scale * HUDManager.ScaleDifference;
        }
    }
}

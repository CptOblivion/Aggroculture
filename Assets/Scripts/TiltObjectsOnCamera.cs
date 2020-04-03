using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TiltObjectsOnCamera : MonoBehaviour
{
    /*
     * Put this on the camera, and put ObjectToTipForCamera on anything that should be tipped
     * Make sure to set script execution order so this runs before objects are activated
     */
    public float DefaultTipAmount = 15;

    public static TiltObjectsOnCamera current;

    public static List<TiltForCamera> TipObjects;

    void Awake()
    {
        TipObjects = new List<TiltForCamera>() { };
        current = this;
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;

    }
    private void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPreRender();
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        OnPostRender();
    }
    void OnPreRender()
    {
        for (int i = 0; i < TipObjects.Count; i++)
        {
            TiltForCamera tipObject = TipObjects[i];
            tipObject.Rotation = tipObject.transform.rotation;
            tipObject.Position = tipObject.transform.position;

            float tipAmount = DefaultTipAmount;
            if (tipObject.OverrideDefaultTipAmount) tipAmount = tipObject.TipAmount;
            tipAmount = (tipAmount + tipObject.ExternalOverride) * tipObject.TipWeight;

            Vector3 position = tipObject.transform.position + new Vector3(0, tipObject.OffsetCenter, 0);

            tipObject.transform.RotateAround(position, transform.right, tipAmount);

            TipObjects[i] = tipObject;
        }
    }

    private void OnPostRender()
    {
        for (int i = 0; i < TipObjects.Count; i++)
        {
            TipObjects[i].transform.rotation = TipObjects[i].Rotation;
            TipObjects[i].transform.position = TipObjects[i].Position;
        }
    }
}

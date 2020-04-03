using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class InteractPrompt : MonoBehaviour
{

    public string TextToDisplay = "Press _Interact_ to use";
    public bool TheRealOne;

    public static InteractPrompt current;
    static Text text;

    private void Awake()
    {
        if (TheRealOne)
        {
            current = this;
            text = GetComponent<Text>();
        }
    }

    private void OnEnable()
    {
        RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;
    }
    private void OnDisable()
    {
        RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
    }
    public void SetInteractText(string phrase)
    {
        if (phrase == null ||phrase == "")
            phrase = TextToDisplay;
        phrase = phrase.Replace("_Interact_", InputBindingsToDisplay.GetDisplayBinding(PlayerMain.current.inputInteract));
        text.text = phrase;
        current.gameObject.SetActive(true);
    }

    private void RenderPipelineManager_endFrameRendering(ScriptableRenderContext context, Camera[] camera)
    {
        AfterRender();
    }
    public void AfterRender()
    {
        current.gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class InteractPrompt : MonoBehaviour
{
  public static InteractPrompt current;
  static Text text;

  private void Awake()
  {
    current = this;
    text = GetComponentInChildren<Text>();
    gameObject.SetActive(false);
  }

  //this function can't be static: unity events need to be able to see it
  public static void SetInteractText(string phrase)
  {
    if (phrase == null || phrase == "")
      phrase = "Text not entered!";
    phrase = phrase.Replace("_Interact_", InputBindingsToDisplay.GetDisplayBinding(PlayerMain.current.inputInteract));
    text.text = phrase;
    current.gameObject.SetActive(true);
  }
  public static void ClearInteractText()
  {
    current.gameObject.SetActive(false);
  }

  /*
  //the old system of displaying text (set it before the frame is rendered, unset it after the frame is rendered, every frame... not good
  private void OnEnable()
  {
      RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;
  }
  private void OnDisable()
  {
      RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
  }

  private void RenderPipelineManager_endFrameRendering(ScriptableRenderContext context, Camera[] camera)
  {
      AfterRender();
  }
  public void AfterRender()
  {
      current.gameObject.SetActive(false);
  }
  */
}

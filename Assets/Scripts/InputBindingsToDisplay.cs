using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputBindingsToDisplay : MonoBehaviour
{
  public static string GetDisplayBinding(InputAction action)
  {
    string text = action.GetBindingDisplayString();
    //eventually there'll be code to insert controller icons here
    return text;
  }
}

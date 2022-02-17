using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTrigger : MonoBehaviour
{
  //for compound bounds, add a playertrigger without a trigger collider, and then use CompoundTriggerEnter and CompoundTriggerExit on other playertriggers to add and remove from collisions
  [HideInInspector]
  public bool Colliding = false;
  public UnityEvent OnEnter;
  public UnityEvent OnExit;

  int CompoundCounter = 0;

  private void OnTriggerEnter(Collider other)
  {
    if (other.gameObject == PlayerMain.current.gameObject)
    {
      TriggerEnter();
    }
  }

  private void OnTriggerExit(Collider other)
  {
    if (other.gameObject == PlayerMain.current.gameObject)
    {
      TriggerExit();
    }
  }

  void TriggerEnter()
  {
    Colliding = true;
    OnEnter.Invoke();
  }

  void TriggerExit()
  {
    Colliding = false;
    OnExit.Invoke();

  }

  public void CompoundTriggerEnter()
  {
    if (CompoundCounter == 0)
    {
      TriggerEnter();
    }
    CompoundCounter++;
  }

  public void CompoundTriggerExit()
  {
    CompoundCounter--;
    if (CompoundCounter == 0)
    {
      TriggerExit();
    }
    else if (CompoundCounter < 0)
    {
      Debug.LogError("Compound collider got too many trigger exits!", this);
    }
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
  public bool Upright = false;
  public bool FlipBasedOnMovement = false;
  Vector3 LastPosition;

  private void Awake()
  {
    LastPosition = transform.position;
  }
  void Update()
  {
    if (Camera.main.orthographic)
    {
      transform.rotation = Camera.main.transform.rotation;
      if (FlipBasedOnMovement && Vector3.Dot(Camera.main.transform.right, transform.position - LastPosition) < 0)
      {
        transform.Rotate(0, 180, 0, Space.Self);
      }
    }
    else
    {
      if (FlipBasedOnMovement)
      {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.current.transform.position * Mathf.Sign(Vector3.Dot(Camera.main.transform.right, transform.position - LastPosition)), Camera.main.transform.up);

      }
      else
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.current.transform.position, Camera.current.transform.up);
    }
    LastPosition = transform.position;
  }
}

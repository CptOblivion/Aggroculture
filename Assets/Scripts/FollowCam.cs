using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
  public float TiltAngle = 60;
  public float TwistAngle = 45;
  public float Size = 15;
  public float Distance = 60;
  public List<Camera> targets;
  [Range(0, 1)]
  public float SmoothSpeed = 0.5f;

  [Range(0, 1)]
  public float OverrideSmoothSpeed = .5f;

  public static FollowCam current;
  [HideInInspector]
  public Camera cam;
  Camera currentTarget;
  float StartingSize;
  // float OverrideFade = 0;
  // float OverrideSpeed = 0;
  Vector3 TargetPosition;

  bool init = false;


  void Awake()
  {
    cam = GetComponent<Camera>();
    StartingSize = cam.orthographicSize;
    current = this;
  }
  private void OnDestroy()
  {
    if (current == this)
      current = null;
  }

  void Update()
  {
    if (!init)
    {
      init = true;
      transform.rotation = Quaternion.Euler(TiltAngle, TwistAngle, 0);
      transform.position = PlayerMain.current.transform.position - (transform.forward * Distance);

    }
    else if (targets.Count > 0)
    {
      currentTarget = targets[targets.Count - 1];
      TargetPosition = currentTarget.transform.position;
      cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, currentTarget.orthographicSize, SmoothSpeed * Time.deltaTime * 60f);
    }
    else
    {
      TargetPosition = PlayerMain.current.transform.position - (transform.forward * Distance);
      cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, StartingSize, SmoothSpeed * Time.deltaTime * 60f);
    }
    transform.rotation = Quaternion.Euler(TiltAngle, TwistAngle, 0);
    transform.position = Vector3.Lerp(transform.position, TargetPosition, SmoothSpeed * Time.deltaTime * 60f);
  }

  public void OverrideLocation(Camera newLocation)
  {
    targets.Add(newLocation);
  }

  public void CancelOverrideLocation(Camera newLocation)
  {
    targets.Remove(newLocation);
  }
}

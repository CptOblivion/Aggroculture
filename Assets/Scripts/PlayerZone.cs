using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerZone : MonoBehaviour
{
    public UnityEvent PlayerEnter;
    public UnityEvent PlayerExit;

    Collider trigger;
    List<GameObject> compositeZoneList;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMain.current.gameObject) PlayerEnter.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMain.current.gameObject) PlayerExit.Invoke();
    }

    public void CompositeZoneAdd(GameObject obj)
    {
        if (compositeZoneList == null) compositeZoneList = new List<GameObject>();
        if (compositeZoneList.Count == 0) PlayerEnter.Invoke();
        compositeZoneList.Add(obj);
    }
    public void CompositeZoneRemove(GameObject obj)
    {
        compositeZoneList.Remove(obj);
        if (compositeZoneList.Count == 0) PlayerExit.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTools : MonoBehaviour
{
    public static Vector2 RotateVector2(Vector2 v, float delta)
    {
        delta = Mathf.Deg2Rad * delta;
        
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTools : MonoBehaviour
{
    public enum Layers {Default, TransparentFX, IgnoreRaycast, Blank1, Water, UI, Blank2, Blank3,
        Player, Monsters, PlayerIFrames, MonsterIFrames, PlayerDamage, MonsterDamage};
public static Vector2 RotateVector2(Vector2 v, float delta)
    {
        delta = Mathf.Deg2Rad * delta;

        
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }
    public static int GenerateLayerMask(int Layer)
    {
        int LayerMask = 0;
        for (int i = 0; i < 32; i++)
        {
            if (!Physics.GetIgnoreLayerCollision(Layer, i))
                LayerMask = LayerMask | 1 << i;
        }
        return LayerMask;
    }
    public static bool PointInHitbox(Vector3 point, Transform transform, Vector3 offset, Vector3 HalfSize)
    {
        //broken for some reason
        point = transform.InverseTransformPoint(point) - offset;

        if (point.x < HalfSize.x && point.x > -HalfSize.x &&
           point.y < HalfSize.y && point.y > -HalfSize.y &&
           point.z < HalfSize.z && point.z > -HalfSize.z)
            return true;
        else
            return false;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeBlendShapes : MonoBehaviour
{
    public bool RandomizeEveryWake = false;
    [Range(0,100)]
    public float[] ShapeRanges;
    bool Init = false;
    private void OnEnable()
    {
        SkinnedMeshRenderer mesh = GetComponent<SkinnedMeshRenderer>();
        if (RandomizeEveryWake || !Init)
        {
            Init = true;
            for (int i = 0; i < ShapeRanges.Length; i++)
            {
                mesh.SetBlendShapeWeight(i, Random.value * ShapeRanges[i]);
            }
        }
    }
}

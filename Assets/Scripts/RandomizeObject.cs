using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeObject : MonoBehaviour
{
  public bool RandomizeEveryWake = false;
  public float RandomX = 0;
  public float RandomY = 0;
  [Range(0, 180)]
  public float RandomRotation = 0;
  [Range(0, 100)]
  public float[] BlendShapes;
  public Mesh[] Meshes;


  Vector3 OriginalPosition;
  bool Init = false;
  private void Awake()
  {
    OriginalPosition = transform.position;
  }
  private void OnEnable()
  {
    SkinnedMeshRenderer mesh = GetComponent<SkinnedMeshRenderer>();
    if (RandomizeEveryWake || !Init)
    {
      Init = true;
      if (Meshes.Length > 0)
      {
        GetComponent<MeshFilter>().mesh = Meshes[Random.Range(0, Meshes.Length)];
      }
      for (int i = 0; i < BlendShapes.Length; i++)
      {
        mesh.SetBlendShapeWeight(i, Random.value * BlendShapes[i]);
      }
      transform.position += new Vector3(Random.Range(-RandomX, RandomX), 0, Random.Range(-RandomY, RandomY));
      transform.Rotate(0, Random.Range(-RandomRotation, RandomRotation), 0, Space.World);
    }
  }
}

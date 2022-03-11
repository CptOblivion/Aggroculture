using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFunctions : MonoBehaviour
{
  public string MaterialColorPropName = "_Color";
  public float alphaFade = 0;
  public void AlphaFade(float speed)
  {
    alphaFade = speed;
  }
  private void Update()
  {
    if (alphaFade != 0)
    {
      //use a negative time to alpha out, positive to alpha in
      bool negative = alphaFade < 0;
      Material mat = gameObject.GetComponent<Renderer>().material;
      Color col = mat.GetColor(MaterialColorPropName);
      if (negative)
      {
        col.a = Mathf.Lerp(col.a, 0, -alphaFade * Time.deltaTime * 60f);
        if (col.a <= 0)
        {
          col.a = 0;
          alphaFade = 0;
        }
      }
      else
      {
        col.a = Mathf.Lerp(col.a, 1, alphaFade * Time.deltaTime * 60f);
        if (col.a >= 1)
        {
          col.a = 1;
          alphaFade = 0;
        }
      }
      mat.SetColor(MaterialColorPropName, col);

    }
  }
}

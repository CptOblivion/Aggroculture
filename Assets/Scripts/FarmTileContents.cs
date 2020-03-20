using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmTileContents : MonoBehaviour
{
    public enum PullAnims {PullWeed, PullRock}
    public PullAnims PullAnimation = PullAnims.PullWeed;
    public GameObject PullEffect;
    public float EffectLife = 0;
    [Tooltip("An object within the pulleffect prefab that should receive the randomizer values from the spawning object")]
    public bool AlignWithPlayer = false;
    [Tooltip("If true, children of AlignWithPlayer will be unparented before aligning and then re-parented after")]
    public bool MaintainChildOrientation = false;
    [Tooltip("For some reason when unparenting, the object is rotated -90 degrees around the X axis. No idea why this is, but this toggle undoes that.")]
    public bool FixRotation = true;
    public void PullFromFarm()
    {
        //Debug.Break();
        if (PullEffect)
        {
            PullEffect = Instantiate(PullEffect);
            PullEffect.transform.position = transform.position;
            PullEffect.transform.rotation = transform.rotation;
            PullEffect.transform.localScale = transform.lossyScale;
            if (EffectLife != 0) Destroy(PullEffect, EffectLife);

            RandomizeObject Randomizer = PullEffect.GetComponent<RandomizeObject>();
            GameObject NewOb = null;
            if (Randomizer)
            {
                foreach (RandomizeObject rand in PullEffect.GetComponentsInChildren<RandomizeObject>())
                {
                    if (rand != Randomizer)
                    {
                        NewOb = rand.gameObject;
                        rand.enabled = false;
                        break;
                    }
                }
                if (NewOb)
                {
                    if (Randomizer.Meshes.Length > 0)
                    {
                        NewOb.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                    }
                    if (Randomizer.BlendShapes.Length > 0)
                    {
                        SkinnedMeshRenderer mesh = GetComponent<SkinnedMeshRenderer>();
                        SkinnedMeshRenderer newMesh = NewOb.GetComponent<SkinnedMeshRenderer>();
                        for (int i = 0; i < Randomizer.BlendShapes.Length; i++)
                        {
                            newMesh.SetBlendShapeWeight(i, mesh.GetBlendShapeWeight(i));
                        }
                    }
                }
            }
            else
            {
                if (PullEffect.GetComponentInChildren<RandomizeObject>()) NewOb = PullEffect.GetComponentInChildren<RandomizeObject>().gameObject;
            }

            if (AlignWithPlayer)
            {
                Transform newObParent = NewOb.transform.parent;
                if (MaintainChildOrientation)
                {
                    NewOb.transform.SetParent(null);
                }
                PullEffect.transform.rotation = Quaternion.LookRotation(PullEffect.transform.position - PlayerMove.currentPlayer.transform.position, Vector3.up);
                if (MaintainChildOrientation)
                { 
                    NewOb.transform.SetParent(newObParent);
                    if (FixRotation) NewOb.transform.Rotate(90, 0, 0);
                }

            }
        }
    }
}

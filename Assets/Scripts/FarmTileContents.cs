using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmTileContents : MonoBehaviour
{
    public string Name;
    public enum PullAnims {NoPull, PullWeed_Big, PullWeed_Small, PullRock}
    public PullAnims PullAnimation = PullAnims.PullWeed_Small;
    public GameObject PullEffect;
    public bool TillWhenPulled = false;
    public float EffectLife = 0;
    [Tooltip("An object within the pulleffect prefab that should receive the randomizer values from the spawning object")]
    public bool AlignWithPlayer = false;
    [Tooltip("If true, children of AlignWithPlayer will be unparented before aligning and then re-parented after")]
    public bool MaintainChildOrientation = false;
    [Tooltip("For some reason when unparenting, the object is rotated -90 degrees around the X axis. No idea why this is, but this toggle undoes that.")]
    public bool FixRotation = true;

    public FarmTileContents GrowsInto;

    public FarmTileContents Spawn(Vector3 Position)
    {
        GameObject newObject = Instantiate(gameObject);
        newObject.transform.position = Position;
        return newObject.GetComponent<FarmTileContents>();

    }
    public void PullFromFarm()
    {
        if (PullEffect)
        {
            RandomizeObject oldRandomizer = GetComponentInChildren<RandomizeObject>();
            Transform oldTransform = transform;
            if (oldRandomizer) oldTransform = oldRandomizer.transform;

            PullEffect = Instantiate(PullEffect, oldTransform.position, oldTransform.rotation);
            if (EffectLife != 0) Destroy(PullEffect, EffectLife);

            RandomizeObject newRandomizer = PullEffect.GetComponent<RandomizeObject>();
            GameObject NewOb = null;
            if (newRandomizer)
            {
                foreach (RandomizeObject rand in PullEffect.GetComponentsInChildren<RandomizeObject>())
                {
                    if (rand != newRandomizer)
                    {
                        NewOb = rand.gameObject;
                        rand.enabled = false;
                        break;
                    }
                }
                if (NewOb)
                {
                    if (newRandomizer.Meshes.Length > 0)
                    {
                        NewOb.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
                    }
                    if (newRandomizer.BlendShapes.Length > 0)
                    {
                        SkinnedMeshRenderer mesh = oldRandomizer.GetComponent<SkinnedMeshRenderer>();
                        SkinnedMeshRenderer newMesh = NewOb.GetComponent<SkinnedMeshRenderer>();
                        for (int i = 0; i < newRandomizer.BlendShapes.Length; i++)
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
                Transform newObParent = null;
                if (MaintainChildOrientation)
                {
                        newObParent = NewOb.transform.parent;
                        NewOb.transform.SetParent(null);
                }
                PullEffect.transform.rotation = Quaternion.LookRotation(PullEffect.transform.position - PlayerMain.current.transform.position, Vector3.up);
                if (MaintainChildOrientation)
                { 
                    NewOb.transform.SetParent(newObParent);
                    if (FixRotation) NewOb.transform.Rotate(90, 0, 0);
                }

            }
        }
    }
}

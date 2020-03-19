using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmPlotEntry
{
    public GameObject Contents = null;
    public string PlannedContents = null;
    public GameObject InitializedContents = null;
    public bool Tilled = false;
    public bool Watered = false;
}

class FarmPlotLerpEntry
{
    public int X;
    public int Y;
    public int Channel;
    public float Progress = 0;
    public float LerpTime;
}

public class FarmPlot : MonoBehaviour
{
    public static Texture2D farmPlotDisplay;
    public FarmPlotEntry[,] FarmContents;
    public int FarmWidth = 32;
    public int FarmHeight = 32;

    public GameObject debugSpawn;

    public float HoeTime = .5f;
    public float WaterTime = 1.5f;

    List<FarmPlotLerpEntry> Lerplist = new List<FarmPlotLerpEntry>();
    bool TileIsTargeted = false;
    Vector3 LastTargetedLocation = Vector3.zero;
    void Awake()
    {

        if (farmPlotDisplay == null)
        {
            farmPlotDisplay = new Texture2D(FarmWidth*2, FarmHeight*2);
            for(int x = 0; x < FarmWidth*2; x++)
            {
                for (int y = 0; y < FarmHeight*2; y++)
                {
                    farmPlotDisplay.SetPixel(x,y,Color.black);
                }
            }
            farmPlotDisplay.Apply();
        }
        FarmContents = new FarmPlotEntry[FarmWidth,FarmHeight];
        for(int x = 0; x < FarmWidth; x++)
        {
            for (int y = 0; y < FarmHeight; y++)
            {
                FarmContents[x, y] = new FarmPlotEntry();
                if (debugSpawn)
                {
                    FarmContents[x, y].Contents = debugSpawn;
                    FillTile(x, y, .2f);
                }
            }
        }
        GetComponent<MeshRenderer>().material.SetTexture("_PlotArray", farmPlotDisplay);
    }

    private void Update()
    {
        for (int i = 0; i < Lerplist.Count; i++)
        {
            FarmPlotLerpEntry entry = Lerplist[i];

            //farmPlotDisplay.SetPixel(TilePosition[0], TilePosition[1], new Color(1, 1, 0));
            Color color = farmPlotDisplay.GetPixel(entry.X * 2, entry.Y * 2);
            color[entry.Channel] = Mathf.SmoothStep(0, 1, entry.Progress);
            farmPlotDisplay.SetPixels(entry.X * 2, entry.Y * 2, 2, 2, new Color[] { color, color, color, color });
            entry.Progress += Time.deltaTime / entry.LerpTime;
            if (entry.Progress > 1)
            {
                Lerplist.RemoveAt(i);
                i--;
            }
            else
            {
                Lerplist[i] = entry;
            }
        }

        if (!TileIsTargeted) TargetTile(LastTargetedLocation, true);


    }
    private void LateUpdate()
    {
        TileIsTargeted = false;
    }

    public bool HoeTile(Vector3 GlobalPosition, bool test)
    {
        /*
         * returns true if the tile is valid to be hoed (and hoeing is initiated), otherwise returns false
         */
        bool start = false;
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (TilePosition[0] >= 0 && TilePosition[0] < FarmWidth && TilePosition[1] >= 0 && TilePosition[1] < FarmWidth)
        {
            FarmPlotEntry entry = FarmContents[TilePosition[0], TilePosition[1]];
            if (!entry.Tilled)
            {
                start = true;
                if (!test)
                {
                    entry.Tilled = true;
                    AddToLerpList(TilePosition[0], TilePosition[1], 0, HoeTime);
                    entry.Contents = null;
                    if (entry.InitializedContents)
                    {
                        Destroy(entry.InitializedContents);
                        entry.InitializedContents = null;
                    }
                    FarmContents[TilePosition[0], TilePosition[1]] = entry;
                }

                //farmPlotDisplay.SetPixel(TilePosition[0], TilePosition[1], new Color(1, 0, 0));
                //farmPlotDisplay.Apply();
            }
        }
        return start;
    }

    public bool WaterTile(Vector3 GlobalPosition, bool test)
    {
        /*
         * returns true if tile is valid to be watered (and watering is initiated), otherwise returns false
         */
        bool start = false;
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (TilePosition[0] >= 0 && TilePosition[0] < 32 && TilePosition[1] >= 0 && TilePosition[1] < 32)
        {
            FarmPlotEntry entry = FarmContents[TilePosition[0], TilePosition[1]];
            if (entry.Tilled && !entry.Watered)
            {
                start = true;
                if (!test)
                {
                    entry.Watered = true;
                    AddToLerpList(TilePosition[0], TilePosition[1], 1, WaterTime);
                    FarmContents[TilePosition[0], TilePosition[1]] = entry;
                }
                //farmPlotDisplay.SetPixel(TilePosition[0], TilePosition[1], new Color(1, 1, 0));
                //farmPlotDisplay.Apply();
            }
        }
        return start;
    }

    void FillTile(int X, int Y, float rand = 0)
    {
        FarmPlotEntry tileEntry = FarmContents[X, Y];
        if (tileEntry.Contents)
        {
            tileEntry.InitializedContents = Instantiate(tileEntry.Contents);
            tileEntry.InitializedContents.transform.position = TileToGlobal(X, Y, rand);
        }
    }
    public void TargetTile(Vector3 GlobalPosition, bool clear = false)
    {
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (TilePosition[0] >= 0 && TilePosition[0] < 32 && TilePosition[1] >= 0 && TilePosition[1] < 32)
        {
            Color color = farmPlotDisplay.GetPixel(TilePosition[0]*2, TilePosition[1]*2);
            if (clear)
            {
                color.b = 0;
            }
            else
            {
                TargetTile(LastTargetedLocation, true);
                LastTargetedLocation = GlobalPosition;
                color.b = 1;
                TileIsTargeted = true;
            }
            farmPlotDisplay.SetPixels(TilePosition[0]*2, TilePosition[1]*2, 2, 2, new Color[] { color, color, color, color });
            farmPlotDisplay.Apply();
        }
    }

    void AddToLerpList(int X, int Y, int Channel, float LerpTime)
    {
        FarmPlotLerpEntry lerpEntry = new FarmPlotLerpEntry();
        lerpEntry.X = X;
        lerpEntry.Y = Y;
        lerpEntry.Channel = Channel;
        lerpEntry.LerpTime = LerpTime;
        Lerplist.Add(lerpEntry);
    }

    int[] GlobalToTile(Vector3 GlobalPosition)
    {
        Vector3 LocalPosition = (transform.InverseTransformPoint(GlobalPosition) * -3.2f);

        int[] TilePosition = new int[] {Mathf.FloorToInt(LocalPosition[0]), Mathf.FloorToInt(LocalPosition[2]) };
        TilePosition[0] += FarmWidth / 2;
        TilePosition[1] += FarmHeight / 2;
        return TilePosition;
    }

    Vector3 TileToGlobal(int X, int Y, float rand = 0)
    {
        X -= FarmWidth / 2;
        Y -= FarmWidth / 2;
        Vector3 Output = new Vector3(X, 0, Y);
        Output += new Vector3(.5f + Random.Range(-rand,rand), 0, .5f + Random.Range(-rand, rand));
        Output = transform.TransformPoint(-Output / 3.2f);
        return Output;
    }
}
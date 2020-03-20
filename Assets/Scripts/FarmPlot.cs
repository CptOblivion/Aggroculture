using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmPlotEntry
{
    public FarmTileContents Contents = null;
    public string PlannedContents = null;
    public FarmTileContents InitializedContents = null;
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

    public bool DebugSpawning = false;
    public FarmTileContents[] DebugSpawns;
    public float[] DebugSpawnPercentages;

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

        if (DebugSpawning && DebugSpawnPercentages.Length != DebugSpawns.Length)
        {
            Debug.LogError("Entries in Debug Spawns and Debug Spawn Percentages don't match!");
            DebugSpawning = false;
        }
        float RandRange = 0;
        for (int i = 0; i < DebugSpawnPercentages.Length; i++) RandRange += DebugSpawnPercentages[i];

        for (int x = 0; x < FarmWidth; x++)
        {
            for (int y = 0; y < FarmHeight; y++)
            {
                FarmContents[x, y] = new FarmPlotEntry();
                if (DebugSpawning)
                {
                    FarmContents[x, y].Contents = (FarmTileContents)PickFromList(DebugSpawns, DebugSpawnPercentages, RandRange);
                    FillTile(x, y);
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

        if (!TileIsTargeted) TargetTileClear(LastTargetedLocation);


    }
    private void LateUpdate()
    {
        TileIsTargeted = false;
    }

    public object PickFromList(object[] List, float[] PickPercentages, float RandRange = 1)
    {
        int i = 0;
        int breakout = 0;
        float Pick = Random.Range(0, RandRange);
        for (; breakout < 100; breakout++)
        {
            Pick -= PickPercentages[i];
            if (Pick <= 0)
            {
                return List[i];
            }
            else
            {
                i++;
                if (i == List.Length) i = 0;
            }
        }
        Debug.LogError($"PickFromList iterated more than {breakout} times, breaking out");
        return List[0];
    }
    public void TargetTile(Vector3 GlobalPosition, InventoryItem.ToolTypes toolType)
    {
        /*
         * Eventually replace with a function to set a different highlight depending on the contents of the tile and the tool selected
         * place an object or project a texture or something, instead of writing a color to the farm plot array
         * eg nice curly-q looking projector when targeting ground with water, something else with hoe, flash contents of tile when targeting with empty hand
         */
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (PointInFarm(TilePosition))
        {
            Color color = farmPlotDisplay.GetPixel(TilePosition[0] * 2, TilePosition[1] * 2);
            TargetTileClear(LastTargetedLocation);
            LastTargetedLocation = GlobalPosition;

            bool ShowTarget = false;
            FarmPlotEntry entry = FarmContents[TilePosition[0], TilePosition[1]];
            if (toolType == InventoryItem.ToolTypes.Hoe && entry.Contents == null && !entry.Tilled) ShowTarget = true;
            else if (toolType == InventoryItem.ToolTypes.WateringCan && entry.Tilled && !entry.Watered) ShowTarget = true;
            else if (toolType == InventoryItem.ToolTypes.Empty && entry.Contents) ShowTarget = true;
            if (ShowTarget)
            {
                color.b = 1;
                TileIsTargeted = true;
                farmPlotDisplay.SetPixels(TilePosition[0] * 2, TilePosition[1] * 2, 2, 2, new Color[] { color, color, color, color });
                farmPlotDisplay.Apply();
            }
        }
    }
    public void TargetTileClear(Vector3 GlobalPosition)
    {
        int[] TilePosition = GlobalToTile(GlobalPosition);

        if (PointInFarm(TilePosition))
        {
            Color color = farmPlotDisplay.GetPixel(TilePosition[0] * 2, TilePosition[1] * 2);
            color.b = 0;
            farmPlotDisplay.SetPixels(TilePosition[0] * 2, TilePosition[1] * 2, 2, 2, new Color[] { color, color, color, color });
            farmPlotDisplay.Apply();
        }
    }

    public string PullTileContents(Vector3 GlobalPosition, bool test = false)
    {
        /*
         * returns true if the tile is valid for weeding/rock removing, otherwise returns false
         */
        string anim = null;
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (PointInFarm(TilePosition))
        {
            FarmPlotEntry entry = FarmContents[TilePosition[0], TilePosition[1]];
            if (entry.Contents)
            {
                if (test)
                {
                    anim = entry.Contents.PullAnimation.ToString();
                }
                else
                {
                    entry.InitializedContents.PullFromFarm();
                    entry.Contents = null;
                    Destroy(entry.InitializedContents.gameObject);
                    entry.InitializedContents = null;
                }
            }
        }
        return anim;
    }

    public bool HoeTile(Vector3 GlobalPosition, bool test)
    {
        /*
         * returns true if the tile is valid to be hoed (and hoeing is initiated), otherwise returns false
         */
        bool start = false;
        int[] TilePosition = GlobalToTile(GlobalPosition);
        if (PointInFarm(TilePosition))
        {
            FarmPlotEntry entry = FarmContents[TilePosition[0], TilePosition[1]];
            if (!entry.Tilled && entry.Contents == null)
            {
                start = true;
                if (!test)
                {
                    entry.Tilled = true;
                    AddToLerpList(TilePosition[0], TilePosition[1], 0, HoeTime);
                    FarmContents[TilePosition[0], TilePosition[1]] = entry;
                }
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
        if (PointInFarm(TilePosition))
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
            //tileEntry.InitializedContents = Instantiate(tileEntry.Contents.gameObject).GetComponent<FarmTileContents>();
            tileEntry.InitializedContents = Instantiate(tileEntry.Contents);
            tileEntry.InitializedContents.transform.position = TileToGlobal(X, Y, rand);
        }
    }

    public bool PointInFarm(int X, int Y)
    {
        return X >= 0 && X < FarmWidth && Y >= 0 && Y < FarmHeight;
    }
    public bool PointInFarm(int[] TilePosition)
    {
        return TilePosition[0] >= 0 && TilePosition[0] < FarmWidth && TilePosition[1] >= 0 && TilePosition[1] < FarmHeight;
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
    void AddToLerpList(int[] TilePosition, int Channel, float LerpTime)
    {
        FarmPlotLerpEntry lerpEntry = new FarmPlotLerpEntry();
        lerpEntry.X = TilePosition[0];
        lerpEntry.Y = TilePosition[1];
        lerpEntry.Channel = Channel;
        lerpEntry.LerpTime = LerpTime;
        Lerplist.Add(lerpEntry);
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
    Vector3 TileToGlobal(int[] TilePosition, float rand = 0)
    {
        TilePosition[0] -= FarmWidth / 2;
        TilePosition[1] -= FarmWidth / 2;
        Vector3 Output = new Vector3(TilePosition[0], 0, TilePosition[1]);
        Output += new Vector3(.5f + Random.Range(-rand, rand), 0, .5f + Random.Range(-rand, rand));
        Output = transform.TransformPoint(-Output / 3.2f);
        return Output;
    }
    int[] GlobalToTile(Vector3 GlobalPosition)
    {
        Vector3 LocalPosition = (transform.InverseTransformPoint(GlobalPosition) * -3.2f);

        int[] TilePosition = new int[] { Mathf.FloorToInt(LocalPosition[0]), Mathf.FloorToInt(LocalPosition[2]) };
        TilePosition[0] += FarmWidth / 2;
        TilePosition[1] += FarmHeight / 2;
        return TilePosition;
    }
}
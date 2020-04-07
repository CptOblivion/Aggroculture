using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    public enum ToolTypes { Empty, Hoe, WateringCan, trowel }
    public bool TwoHanded = false;
    public ToolTypes toolType = ToolTypes.Empty;
    public string[] Tags = new string[0];
}

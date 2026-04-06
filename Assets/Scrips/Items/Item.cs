using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public ItemType type;

    [Header("Held Object")]
    public GameObject heldPrefab;

    [Header("Inventory Object")]
    public Sprite image;
    public Sprite imageR90; // optional; if null we reuse image
    public Texture2D itemIcon;
    public int sizeWidth = 1;
    public int sizeHeight = 1;

    [Header("Inventory Shape (Optional)")]
    [Tooltip("If empty, the item occupies a solid rectangle sizeWidth x sizeHeight. " +
             "If set, each entry is a tile offset inside the bounding box (0..sizeWidth-1, 0..sizeHeight-1) that is occupied.")]
    public Vector2Int[] occupiedCells;

    [Header("Gameplay")]
    public ActionType actionType;


    private void OnValidate()
    {
        sizeWidth = Mathf.Max(1, sizeWidth);
        sizeHeight = Mathf.Max(1, sizeHeight);

        if (occupiedCells == null || occupiedCells.Length == 0)
            return;

        // Warn if any cell is outside bounds
        for (int i = 0; i < occupiedCells.Length; i++)
        {
            Vector2Int c = occupiedCells[i];
            if (c.x < 0 || c.y < 0 || c.x >= sizeWidth || c.y >= sizeHeight)
            {
                Debug.LogWarning(
                    $"{name}: occupiedCells[{i}] = {c} is outside bounds (0..{sizeWidth - 1}, 0..{sizeHeight - 1}). It will be ignored at runtime.",
                    this);
            }
        }
    }
}

public enum ItemType
{
    Loot,
    Utility,
    KeyItem,
    Armor,
}

public enum ActionType
{
    Flashlight,
    Hit,
    Open,
    Use,
    Pickup,
}


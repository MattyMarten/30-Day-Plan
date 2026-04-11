using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public ItemType type;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("Held Object")]
    public GameObject heldPrefab;

    [Header("World Drop")]
    public GameObject dropPrefab;

    [Header("Inventory Object")]
    public Sprite image;
    public Sprite imageR90;
    public Texture2D itemIcon;
    public int sizeWidth = 1;
    public int sizeHeight = 1;

    [Header("Inventory Shape (Optional)")]
    public Vector2Int[] occupiedCells;

    [Header("Gameplay")]
    public ActionType actionType;

    [Serializable]
    public struct MaterialAmount
    {
        public RawMaterial material;
        public int amount;
    }

    [Header("Material Value")]
    public List<MaterialAmount> materialAmounts = new List<MaterialAmount>();

    [HideInInspector]
    public Dictionary<RawMaterial, int> materialValue = new Dictionary<RawMaterial, int>();

    private void OnValidate()
    {
        sizeWidth = Mathf.Max(1, sizeWidth);
        sizeHeight = Mathf.Max(1, sizeHeight);

        if (occupiedCells == null || occupiedCells.Length == 0)
            return;

        for (int i = 0; i < occupiedCells.Length; i++)
        {
            Vector2Int c = occupiedCells[i];
            if (c.x < 0 || c.y < 0 || c.x >= sizeWidth || c.y >= sizeHeight)
            {
                Debug.LogWarning($"{name}: occupiedCells[{i}] = {c} is outside bounds (0..{sizeWidth - 1}, 0..{sizeHeight - 1}). It will be ignored at runtime.", this);
            }
        }

        // Build the dictionary:
        materialValue.Clear();
        foreach (var entry in materialAmounts)
        {
            if (entry.amount > 0)
                materialValue[entry.material] = entry.amount;
        }
    }
}

public enum ItemType { Loot, Utility, KeyItem, Armor }
public enum ActionType { Flashlight, Hit, Open, Use, Pickup }
public enum ItemRarity { Common, Uncommon, Rare, Legendary, Cursed }
public enum RawMaterial { None, Wood, Stone, Metal, Cloth, Leather, Iron }
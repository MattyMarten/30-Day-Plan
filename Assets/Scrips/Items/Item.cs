using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public ItemType type;
    public ItemRarity rarity = ItemRarity.Junk;
    public Color RarityColor
{
    get
    {
        switch (rarity)
        {
            case ItemRarity.Junk:      return new Color(0.5f, 0.5f, 0.5f, 1f); // gray
            case ItemRarity.Valuable:  return new Color(0.2f, 0.8f, 0.3f, 0.25f); // green
            case ItemRarity.Unusual:   return new Color(0.25f, 0.55f, 1.0f, 0.25f); // blue
            case ItemRarity.Exotic:    return new Color(1.00f, 0.80f, 0.15f, 0.25f); // gold
            case ItemRarity.Cursed:    return new Color(0.7f, 0.2f, 0.3f, 0.25f); // dark red/purple
            default:                   return Color.white;
        }
    }
}

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

    public Dictionary<RawMaterial, int> MaterialValue
    {
        get
        {
            var dict = new Dictionary<RawMaterial, int>();
            foreach (var entry in materialAmounts)
            {
                if (entry.amount > 0)
                    dict[entry.material] = entry.amount;
            }
            return dict;
        }
    }

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
    } 
}

public enum ItemType { Loot, Utility, KeyItem, Armor }
public enum ActionType { Flashlight, Hit, Open, Use, Pickup }
public enum ItemRarity { Junk, Valuable, Unusual, Exotic, Cursed }
public enum RawMaterial { None, Wood, Scrap, Cloth, Glass, Rubber, Plastic, Paper}
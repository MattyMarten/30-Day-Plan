using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftingRecipe
{
    public string displayName;
    public Sprite displayIcon;
    public List<MaterialRequirement> requiredMaterials;
    public int valueGold;
}

[System.Serializable]
public class MaterialRequirement
{
    public RawMaterial material;
    public int amount;
}
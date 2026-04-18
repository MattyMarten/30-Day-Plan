using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftingRecipe
{
    public Sprite displayIcon;
    public string displayName;
    public string description;
    public List<MaterialRequirement> requiredMaterials;   // This must be public and match the code!
    public int valueGold;

    [System.Serializable]
    public class MaterialRequirement
    {
        public RawMaterial material;
        public int amount;
    }
}
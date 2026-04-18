using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Scriptable Objects/CraftingRecipes")]
public class CraftingRecipes : ScriptableObject
{
    public Item resultItem;
    public int resultAmount = 1;

    [System.Serializable]
    public struct MaterialRequirement
    {
        public RawMaterial material;
        public int amount;
    }

    public List<MaterialRequirement> requiredMaterials = new List<MaterialRequirement>();
    [TextArea] public string description;
}

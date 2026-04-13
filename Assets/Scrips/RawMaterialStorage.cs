using UnityEngine;
using System.Collections.Generic;

public class RawMaterialStorage : MonoBehaviour
{
    private Dictionary<RawMaterial, int> materialCounts = new();

    public void Add(RawMaterial type, int amount)
    {
        if (amount <= 0) return;
        if (!materialCounts.ContainsKey(type))
            materialCounts[type] = 0;
        materialCounts[type] += amount;
    }

    public bool TrySpend(RawMaterial type, int amount)
    {
        if (!materialCounts.ContainsKey(type) || materialCounts[type] < amount) return false;
        materialCounts[type] -= amount;
        return true;
    }

    public Dictionary<RawMaterial, int> GetAll()
    {
        // Always return a copy so UI can't change your internal storage!
        return new Dictionary<RawMaterial, int>(materialCounts);
    }
}
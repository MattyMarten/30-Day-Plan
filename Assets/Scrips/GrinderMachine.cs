using UnityEngine;
using System.Collections.Generic;
using TMPro; // For UI

public class GrinderMachine : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory; // Drag in Editor or auto-find
    public GridInventory gridInventory;     // Add this reference if you want to auto-refresh grid UI

    [Header("UI")]
    public GameObject summaryPanel;
    public TMP_Text summaryText;

    void Awake()
    {
        if (playerInventory == null)
            playerInventory = FindObjectOfType<PlayerInventory>();
        if (gridInventory == null)
            gridInventory = FindObjectOfType<GridInventory>();
    }

    // Call this function (e.g., from a button) to grind all inventory
    public void GrindAllItems()
    {
        if (playerInventory == null) return;

        // Track what materials we got
        Dictionary<RawMaterial, int> resultMaterials = new();

        // We'll keep track of which items we've already removed (for multi-cell)
        HashSet<InventoryLoot> removedLoot = new();

        for (int x = 0; x < playerInventory.gridWidth; x++)
        for (int y = 0; y < playerInventory.gridHeight; y++)
        {
            var loot = playerInventory.gridItems[x, y];
            if (loot != null && loot.item != null && !removedLoot.Contains(loot))
            {
                // Add materials for this item
                foreach (var pair in loot.item.MaterialValue)
                {
                    if (!resultMaterials.ContainsKey(pair.Key))
                        resultMaterials[pair.Key] = 0;
                    resultMaterials[pair.Key] += pair.Value;
                }
                // Remove from all cells and prevent double-count
                playerInventory.RemoveMultiCellItem(loot);
                removedLoot.Add(loot);
            }
        }

        // Show result in UI
        ShowSummary(resultMaterials);
        // Optionally, trigger inventory UI grid refresh here if needed
        if (gridInventory != null)
            gridInventory.RefreshGridUI();
    }

    public void ShowSummary(Dictionary<RawMaterial, int> materials)
    {
        if (summaryPanel != null) summaryPanel.SetActive(true);

        if (summaryText != null)
        {
            if (materials == null || materials.Count == 0)
            {
                summaryText.text = "No materials received!";
                return;
            }

            string summary = "You received:\n";
            foreach (var kvp in materials)
            {
                summary += $"{kvp.Value}x {kvp.Key}\n";
            }
            summaryText.text = summary;
        }
    }

    // Optionally, a button to close the panel
    public void HideSummary()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false);
    }
}
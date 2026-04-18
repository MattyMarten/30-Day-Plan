using UnityEngine;
using System.Collections.Generic;
using TMPro; // For UI
using StarterAssets;

public class GrinderMachine : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory; // Drag in Editor or auto-find
    public GridInventory gridInventory;     // Add this reference if you want to auto-refresh grid UI
    public StarterAssetsInputs input;
    public RawMaterialStorage storage;

    [Header("UI")]
    public GameObject summaryPanel;
    public TMP_Text summaryText;

    [Header("Timer")]
    public float messageTimer = 20f; // How long to show the results (seconds)

    private float messageTimerEnd = -1f; // Internal: time when the panel should hide



   void Awake()
    {
        if (summaryPanel != null) summaryPanel.SetActive(false); // Start hidden
    }

    void Update()
    {
        if (summaryPanel != null && summaryPanel.activeSelf)
        {
            // Close if timer expired
            if (messageTimerEnd > 0f && Time.time >= messageTimerEnd)
                HideSummary();

            // Close if player presses X (or whatever SkipMessage is)
            if (input.ConsumeSkipMessage())
                HideSummary();
        }
    }


public void Grind()
{
    if (playerInventory == null || storage == null) return;

    // Copy logic from GrindAllItems here ↓↓↓
    Dictionary<RawMaterial, int> resultMaterials = new();
    HashSet<InventoryLoot> removedLoot = new();

    int width = playerInventory.gridItems.GetLength(0);
    int height = playerInventory.gridItems.GetLength(1);

    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            var loot = playerInventory.gridItems[x, y];
            if (loot != null && loot.item != null && !removedLoot.Contains(loot))
            {
                foreach (var pair in loot.item.MaterialValue)
                {
                    if (!resultMaterials.ContainsKey(pair.Key))
                        resultMaterials[pair.Key] = 0;
                    resultMaterials[pair.Key] += pair.Value;
                }
                playerInventory.RemoveMultiCellItem(loot);
                removedLoot.Add(loot);
            }
        }

    // Add results to storage
    foreach (var kvp in resultMaterials)
        storage.Add(kvp.Key, kvp.Value);

    foreach (var kvp in storage.GetAll())
        Debug.Log($"Storage: {kvp.Key}: {kvp.Value}");

    var storageUI = FindAnyObjectByType<MaterialStorageUI>();
    if (storageUI != null && storageUI.gameObject.activeInHierarchy)
    {
        storageUI.RefreshUI();
    }

    ShowSummary(resultMaterials);

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
                summary += $"{kvp.Value}x {kvp.Key.displayName}\n";
            }
            summaryText.text = summary;
        }

        // Start the timer
        messageTimerEnd = Time.time + messageTimer;
    }

    public void HideSummary()
    {
        if (summaryPanel != null)
        {
            summaryPanel.SetActive(false);
            messageTimerEnd = -1f; // Stop the timer
        }
    }
}
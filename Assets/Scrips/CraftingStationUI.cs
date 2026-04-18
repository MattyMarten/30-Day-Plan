using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    // Right panel fields (assign these in the inspector)
    public GameObject detailPanel;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public TMP_Text detailRequirementsText; // requirements and have text
    public TMP_Text detailValueText;
    public Slider detailAmountSlider;
    public TMP_Text detailAmountText;
    public Button detailCraftButton;

    // Used to track/hold detail state
    private CraftingRecipe selectedRecipe;
    private int maxCraftable;
    
    [Header("Left panel/recipe list")]
    public Transform contentParent;
    public GameObject craftingRowPrefab;

    [Header("Player data")]
    public RawMaterialStorage playerStorage;
    public List<CraftingRecipe> recipes;

    private List<GameObject> spawnedRows = new();

    void Start() => RefreshUI();

    public void RefreshUI()
    {
        foreach (var go in spawnedRows)
            Destroy(go);
        spawnedRows.Clear();

        foreach (var recipe in recipes)
        {
            var go = Instantiate(craftingRowPrefab, contentParent);
            // Set item name (left panel)
            go.transform.Find("ItemName").GetComponent<TMP_Text>().text = recipe.displayName;
            // Set item icon if you want (for the LEFT only)
            var image = go.transform.Find("ItemImage");
            if (image) image.GetComponent<Image>().sprite = recipe.displayIcon;

            var btn = go.GetComponent<Button>();
            var capturedRecipe = recipe;
            btn.onClick.AddListener(() => OnSelectRecipe(capturedRecipe));
            spawnedRows.Add(go);
        }
    }

    void OnSelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        detailPanel.SetActive(true);

        // detailIcon.sprite = recipe.displayIcon; // <-- removed!
        detailName.text = recipe.displayName;
        detailDesc.text = recipe.description;
        detailValueText.text = $"Value: {recipe.valueGold}G";

        // Format requirements/have blocks
        var needSB = new System.Text.StringBuilder();
        needSB.Append("Need: ");
        foreach (var req in recipe.requiredMaterials)
        {
            needSB.Append($"{req.amount}x {req.material.displayName} ");
        }
        needSB.AppendLine();

        needSB.Append("Have: ");
        foreach (var req in recipe.requiredMaterials)
        {
            int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
            needSB.Append($"{have}x {req.material.displayName} ");
        }

        detailRequirementsText.text = needSB.ToString();

        // Crafting amount logic
        maxCraftable = int.MaxValue;
        foreach (var req in recipe.requiredMaterials)
        {
            int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
            int max = req.amount > 0 ? have / req.amount : 0;
            if (max < maxCraftable) maxCraftable = max;
        }
        maxCraftable = Mathf.Max(0, maxCraftable);

        detailAmountSlider.minValue = 1;
        detailAmountSlider.maxValue = Mathf.Max(1, maxCraftable);
        detailAmountSlider.wholeNumbers = true;
        detailAmountSlider.value = maxCraftable > 0 ? 1 : 0;

        UpdateDetailAmountText((int)detailAmountSlider.value);
        detailAmountSlider.onValueChanged.RemoveAllListeners();
        detailAmountSlider.onValueChanged.AddListener((val) => UpdateDetailAmountText((int)val));

        detailCraftButton.interactable = maxCraftable > 0;
        detailCraftButton.onClick.RemoveAllListeners();
        detailCraftButton.onClick.AddListener(() => TryCraftSelected((int)detailAmountSlider.value));
    }

    void UpdateDetailAmountText(int val)
    {
        detailAmountText.text = $"x{val}";
    }

    void TryCraftSelected(int count)
    {
        if (selectedRecipe == null) return;
        for (int i = 0; i < count; i++)
        {
            bool canCraft = true;
            foreach (var req in selectedRecipe.requiredMaterials)
            {
                int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
                if (have < req.amount)
                {
                    canCraft = false;
                    break;
                }
            }
            if (!canCraft) break;
            foreach (var req in selectedRecipe.requiredMaterials)
                playerStorage.TrySpend(req.material, req.amount);
            // TODO: Add crafted item to inventory!
        }
        RefreshUI();
        if (selectedRecipe != null) OnSelectRecipe(selectedRecipe);
    }
}
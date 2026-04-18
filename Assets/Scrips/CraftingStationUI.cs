using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
    // Right panel fields (assign these in the inspector)
    public GameObject detailPanel;
    public Image detailIcon;
    public TMP_Text detailName;
    public TMP_Text detailDesc;
    public Transform detailRequirementsRoot;    // Parent for spawnable material requirements
    public GameObject detailRequirementRowPrefab; // Prefab: {icon} {“You have/need”}
    public TMP_Text detailValueText;
    public Slider detailAmountSlider;
    public TMP_Text detailAmountText;
    public Button detailCraftButton;

    // Used to track/hold detail state
    private CraftingRecipe selectedRecipe;
    private int maxCraftable;
    
    public Transform contentParent;
    public GameObject craftingRowPrefab;
    public RawMaterialStorage playerStorage;
    public List<CraftingRecipe> recipes; // ← This is the only list you need

    private List<GameObject> spawnedRows = new();

    public void RefreshUI()
    {
        foreach (var go in spawnedRows)
            Destroy(go);
        spawnedRows.Clear();

        var canCraft = new List<CraftingRecipe>();
        var cannotCraft = new List<CraftingRecipe>();

        foreach (var recipe in recipes)
        {
            var go = Instantiate(craftingRowPrefab, contentParent);

            go.transform.Find("ItemName").GetComponent<TMP_Text>().text = recipe.displayName;
            go.transform.Find("ItemImage").GetComponent<Image>().sprite = recipe.displayIcon;

            var btn = go.GetComponent<Button>();
            var capturedRecipe = recipe;
            btn.onClick.AddListener(() => OnSelectRecipe(capturedRecipe));
            spawnedRows.Add(go);
        }

        foreach (var recipe in canCraft.Concat(cannotCraft))
        {
            var go = Instantiate(craftingRowPrefab, contentParent);

            go.transform.Find("Icon Part/Item Icon").GetComponent<Image>().sprite = recipe.displayIcon;
            go.transform.Find("Text Part/Item Name").GetComponent<TMP_Text>().text = recipe.displayName;

            // Build requirements & have/needs string
            var reqText = go.transform.Find("Text Part/Requirements").GetComponent<TMP_Text>();
            string needs = string.Join(", ", recipe.requiredMaterials.Select(r => $"{r.amount}x {r.material}"));
            string haves = string.Join(", ", recipe.requiredMaterials.Select(r =>
            {
                int have = playerStorage.GetAll().TryGetValue(r.material, out int amt) ? amt : 0;
                return $"{have}x {r.material}";
            }));
            reqText.text = $"Need: {needs}\nHave: {haves}";

            // Craft Button
            var btn = go.transform.Find("Craft Button").GetComponent<Button>();
            var btnText = go.transform.Find("Craft Button/Craft").GetComponent<TMP_Text>();
            var valText = go.transform.Find("Text Part/Value").GetComponent<TMP_Text>();

            btnText.text = "Craft";
            valText.text = $"Value: {recipe.valueGold}G";

            if (canCraft.Contains(recipe))
            {
                btn.interactable = true;
                btn.GetComponent<Image>().color = Color.green;
            }
            else
            {
                btn.interactable = false;
                btn.GetComponent<Image>().color = Color.red;
            }

            var capturedRecipe = recipe;
            btn.onClick.AddListener(() => OnSelectRecipe(capturedRecipe));
            spawnedRows.Add(go);
        }
    }
    void OnSelectRecipe(CraftingRecipe recipe)
    {
        selectedRecipe = recipe;
        detailPanel.SetActive(true);

        detailIcon.sprite = recipe.displayIcon;
        detailName.text = recipe.displayName;
        detailDesc.text = recipe.description;
        detailValueText.text = $"Value: {recipe.valueGold}G";

        // Clear old requirement rows
        foreach (Transform child in detailRequirementsRoot)
            Destroy(child.gameObject);

        // Find min possible crafts based on requirements
        maxCraftable = int.MaxValue; // Start large
        foreach (var req in recipe.requiredMaterials)
        {
            int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
            int max = req.amount > 0 ? have / req.amount : 0;
            if (max < maxCraftable) maxCraftable = max;

            // Spawn req row
            var reqGO = Instantiate(detailRequirementRowPrefab, detailRequirementsRoot);
            reqGO.transform.Find("Icon").GetComponent<Image>().sprite = req.material.icon;
            reqGO.transform.Find("Text").GetComponent<TMP_Text>().text =
                    $"You have: {have} / Need: {req.amount}";
        }

        // Clamp to at least 1
        maxCraftable = Mathf.Max(0, maxCraftable);

        // Update slider min/max/value
        detailAmountSlider.minValue = 1;
        detailAmountSlider.maxValue = Mathf.Max(1, maxCraftable);
        detailAmountSlider.wholeNumbers = true;
        detailAmountSlider.value = maxCraftable > 0 ? 1 : 0;

        // Show amount text and update on slider move
        UpdateDetailAmountText((int)detailAmountSlider.value);
        detailAmountSlider.onValueChanged.RemoveAllListeners();
        detailAmountSlider.onValueChanged.AddListener((val) => UpdateDetailAmountText((int)val));

        // Craft button enabled only if craftable
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
        // Check again in case player's storage changed since UI was opened!
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
        // Refresh UI on left and right
        RefreshUI();
        if (selectedRecipe != null) OnSelectRecipe(selectedRecipe);
    }
}
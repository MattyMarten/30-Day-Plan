using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class CraftingStationUI : MonoBehaviour
{
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
            bool hasAll = true;
            foreach (var req in recipe.requiredMaterials)
            {
                int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
                if (have < req.amount)
                    hasAll = false;
            }
            if (hasAll) canCraft.Add(recipe);
            else cannotCraft.Add(recipe);
        }

        foreach (var recipe in canCraft.Concat(cannotCraft))
        {
            var go = Instantiate(craftingRowPrefab, contentParent);

            go.transform.Find("Item Icon").GetComponent<Image>().sprite = recipe.displayIcon;
            go.transform.Find("Item Name").GetComponent<TMP_Text>().text = recipe.displayName;

            // Build requirements & have/needs string
            var reqText = go.transform.Find("Requirements").GetComponent<TMP_Text>();
            string needs = string.Join(", ", recipe.requiredMaterials.Select(r => $"{r.amount}x {r.material}"));
            string haves = string.Join(", ", recipe.requiredMaterials.Select(r =>
            {
                int have = playerStorage.GetAll().TryGetValue(r.material, out int amt) ? amt : 0;
                return $"{have}x {r.material}";
            }));
            reqText.text = $"Need: {needs}\nHave: {haves}";

            // Craft Button
            var btn = go.transform.Find("Craft").GetComponent<Button>();
            var btnText = go.transform.Find("Craft/Craft").GetComponent<TMP_Text>();
            var valText = go.transform.Find("Craft/Value").GetComponent<TMP_Text>();

            btnText.text = "Craft";
            valText.text = $"Value: {recipe.valueGold}gold";

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
            btn.onClick.AddListener(() => TryCraft(capturedRecipe));
            spawnedRows.Add(go);
        }
    }

    void TryCraft(CraftingRecipe recipe)
    {
        // Try to spend requirements
        foreach (var req in recipe.requiredMaterials)
        {
            int have = playerStorage.GetAll().TryGetValue(req.material, out int amt) ? amt : 0;
            if (have < req.amount)
                return;
        }
        foreach (var req in recipe.requiredMaterials)
            playerStorage.TrySpend(req.material, req.amount);

        // TODO: Add crafted item to inventory here!
        Debug.Log($"Crafted {recipe.displayName}!");

        RefreshUI();
    }
}
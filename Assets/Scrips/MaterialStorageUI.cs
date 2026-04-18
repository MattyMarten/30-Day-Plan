using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MaterialStorageUI : MonoBehaviour
{
    public RawMaterialStorage storage;       // Assign in inspector
    public Transform contentRoot;            // Assign to your content/gameobject holding rows
    public GameObject rowPrefab;             // Assign your MaterialRowPrefab

    private List<GameObject> spawnedRows = new();

    public void RefreshUI()
    {
        // Remove old rows
        foreach (var row in spawnedRows)
            Destroy(row);
        spawnedRows.Clear();

        if (storage == null) return;
        Dictionary<RawMaterial, int> all = storage.GetAll();
        foreach (var kv in all)
        {
            var go = Instantiate(rowPrefab, contentRoot);

            var nameText = go.transform.Find("materialNameText").GetComponent<TMP_Text>();
            var amountText = go.transform.Find("materialAmountText").GetComponent<TMP_Text>();

            nameText.text = kv.Key.displayName;    // Use displayName property!
            amountText.text = "x" + kv.Value;

            spawnedRows.Add(go);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }
}


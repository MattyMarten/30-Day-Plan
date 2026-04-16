using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipPanel : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI materialText;

    private void Awake()
    {
        gameObject.SetActive(false); // Hide tooltip by default
    }
    /// Call this to update and display the tooltip
    public void Show(string itemName, string rarity, string materials, Vector2 screenPosition)
    {
        itemNameText.text = itemName;
        rarityText.text = rarity;
        materialText.text = materials;

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Set position early (optional)
        transform.position = screenPosition + new Vector2(20, -20);

        // Now defer layout update until end of frame
        StartCoroutine(RefreshLayoutNextFrame());
    }

    private System.Collections.IEnumerator RefreshLayoutNextFrame()
    {
        yield return null; // wait one frame
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
    }

    public void Hide() => gameObject.SetActive(false);
}

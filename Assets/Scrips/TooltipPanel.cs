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
    // Start update coroutine to refresh after one frame:
    if (!gameObject.activeSelf) gameObject.SetActive(true);
    StartCoroutine(RefreshLayout(screenPosition));
}

private System.Collections.IEnumerator RefreshLayout(Vector2 screenPosition)
{
    yield return null; // Wait one frame so Unity calculates preferred sizes
    LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    // Now move the tooltip (after sizing is correct!)
    transform.position = screenPosition + new Vector2(20, -20);
}

    public void Hide() => gameObject.SetActive(false);
}

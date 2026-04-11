using UnityEngine;
using TMPro;

public class TooltipPanel : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI materialText;

    /// Call this to update and display the tooltip
    public void Show(string itemName, string rarity, string materials, Vector2 screenPosition)
    {
        itemNameText.text = itemName;
        rarityText.text = rarity;
        materialText.text = materials;
        
        // Move tooltip near mouse, with a small offset
        transform.position = screenPosition + new Vector2(20, -20);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private Image image;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color notSelectedColor = Color.gray;

    private void Awake()
    {
        // Auto-find the Image if not assigned (optional, but reduces setup mistakes)
        if (image == null)
            image = GetComponent<Image>();

        Deselect();
    }

    public void Select()
    {
        if (image != null)
            image.color = selectedColor;
    }

    public void Deselect()
    {
        if (image != null)
            image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null)
            return;

        DraggableItem dragged = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (dragged == null)
            return;

        Transform fromSlot = dragged.parentAfterDrag; // original slot
        Transform toSlot = transform;

        if (fromSlot == null)
            return;

        // Find existing item in target slot (direct child)
        DraggableItem existing = null;
        for (int i = 0; i < toSlot.childCount; i++)
        {
            var di = toSlot.GetChild(i).GetComponent<DraggableItem>();
            if (di != null) { existing = di; break; }
        }

        // If occupied -> swap
        if (existing != null && existing != dragged)
        {
            existing.transform.SetParent(fromSlot, false);
            var existingRT = existing.GetComponent<RectTransform>();
            if (existingRT != null) existingRT.anchoredPosition = Vector2.zero;
        }

        // Drop dragged into this slot
        dragged.parentAfterDrag = toSlot;
    }
}
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
        if (eventData == null)
            return;

        if (eventData.pointerDrag == null)
            return;

        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem == null)
            return;

        // BLOCK dropping onto occupied slots for now (safe default).
        // Later we can implement swapping.
        DraggableItem existingItem = GetComponentInChildren<DraggableItem>();
        if (existingItem != null && existingItem != draggableItem)
        {
            // Don't change parentAfterDrag => item will snap back to original slot.
            return;
        }

        draggableItem.parentAfterDrag = transform;
    }
}
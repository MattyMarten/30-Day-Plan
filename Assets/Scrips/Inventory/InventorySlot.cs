using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;
    
    void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }
        public void Deselect()
        {
            image.color = notSelectedColor;
        }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            draggableItem.parentAfterDrag = transform;
        }
    }
}

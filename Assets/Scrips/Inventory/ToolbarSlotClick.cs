using UnityEngine;
using UnityEngine.EventSystems;

public class ToolbarSlotClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private int slotIndex;

    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = GetComponentInParent<InventoryManager>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager == null)
        {
            Debug.LogWarning($"{nameof(ToolbarSlotClick)}: No InventoryManager assigned.", this);
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        inventoryManager.SelectSlot(slotIndex);
    }
}

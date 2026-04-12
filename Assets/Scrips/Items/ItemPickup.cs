using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private Item item;

    [Header("Optional")]
    [Tooltip("If true, item will also equip/hold the heldPrefab when picked up.")]
    [SerializeField] private bool autoHoldOnPickup = true;

    public Item Item => item;

    public void SetItem(Item newItem) => item = newItem;

    public void PickUp(InventoryManager inventoryManager, PlayerHoldItem playerHoldItem)
    {
        if (item == null)
        {
            Debug.LogWarning($"{nameof(ItemPickup)}: No item assigned on pickup.", this);
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning($"{nameof(ItemPickup)}: inventoryManager reference is null.", this);
            return;
        }

        bool added = inventoryManager.AddItem(item);
        if (!added)
        {
            Debug.Log("Inventory full");
            return;
        }

        if (autoHoldOnPickup && playerHoldItem != null && item.heldPrefab != null)
        {
            playerHoldItem.HoldItem(item.heldPrefab);
        }

        Debug.Log("Picked up: " + item.itemName);
        Destroy(gameObject);
    }
}
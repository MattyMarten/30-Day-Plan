using UnityEngine;
using StarterAssets;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    private bool playerInRange;
    private StarterAssetsInputs input;
    private PlayerHoldItem playerHoldItem;
    private InventoryManager inventoryManager;

    private void Update()
    {
        if (playerInRange && input != null && input.interact)
        {
            PickUp();
            input.interact = false;
        }
    }

 public void PickUp()
    {
        if (item == null) return;
        if (playerHoldItem == null) return;
        if (inventoryManager == null) return;

        bool added = inventoryManager.AddItem(item);

        if (added)
        {
            if (item.heldPrefab != null)
            {
                playerHoldItem.HoldItem(item.heldPrefab);
            }

            Debug.Log("Picked up: " + item.itemName);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Inventory full");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerHoldItem = other.GetComponent<PlayerHoldItem>();
            input = other.GetComponent<StarterAssetsInputs>();
            inventoryManager = FindAnyObjectByType<InventoryManager>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerHoldItem = null;
            input = null;
        }
    }
}
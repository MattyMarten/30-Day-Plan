using UnityEngine;
using StarterAssets;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Loot Grid Inventory (assign in inspector)")]
    [SerializeField] private GridInventory gridLootInventory;

    [Header("References (assign in inspector)")]
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private PlayerHoldItem playerHoldItem;

    [Header("UI")]
    [SerializeField] private Vector2 itemUISize = new Vector2(80, 80);

    [Header("Messages")]
    [Tooltip("A GameObject that contains your NO Space text (setActive true/false).")]
    [SerializeField] private GameObject noSpaceMessage;
    [SerializeField] private float noSpaceMessageSeconds = 1.5f;

    private float noSpaceHideAt;
    private int selectedSlot = -1;

    private void Awake()
    {
        if (noSpaceMessage != null)
            noSpaceMessage.SetActive(false);

        if (inventorySlots != null && inventorySlots.Length > 0)
            ChangeSelectedSlot(0);
    }

    private void Update()
    {
        // Hide message after timeout
        if (noSpaceMessage != null && noSpaceMessage.activeSelf && Time.time >= noSpaceHideAt)
            noSpaceMessage.SetActive(false);

        if (inventorySlots == null || inventorySlots.Length == 0)
            return;

        if (input == null)
            return;

        HandleNumberInput();
        HandleScrollInput();
    }

    private void ShowNoSpaceMessage()
    {
        if (noSpaceMessage == null)
        {
            Debug.Log("NO Space");
            return;
        }

        noSpaceMessage.SetActive(true);
        noSpaceHideAt = Time.time + noSpaceMessageSeconds;
    }

    private void HandleNumberInput()
    {
        if (input.Slot1) ChangeSelectedSlotIfValid(0);
        else if (input.Slot2) ChangeSelectedSlotIfValid(1);
        else if (input.Slot3) ChangeSelectedSlotIfValid(2);
        else if (input.Slot4) ChangeSelectedSlotIfValid(3);
        else if (input.Slot5) ChangeSelectedSlotIfValid(4);
        else if (input.Slot6) ChangeSelectedSlotIfValid(5);
        else if (input.Slot7) ChangeSelectedSlotIfValid(6);
        else if (input.Slot8) ChangeSelectedSlotIfValid(7);
        else if (input.Slot9) ChangeSelectedSlotIfValid(8);
        else if (input.Slot0) ChangeSelectedSlotIfValid(9);
    }

    private void HandleScrollInput()
    {
        if (input.scroll == 0f)
            return;

        int direction = input.scroll > 0 ? 1 : -1;

        int current = selectedSlot >= 0 ? selectedSlot : 0;
        int newSlot = current + direction;

        if (newSlot >= inventorySlots.Length)
            newSlot = 0;
        else if (newSlot < 0)
            newSlot = inventorySlots.Length - 1;

        ChangeSelectedSlot(newSlot);
    }

    private void ChangeSelectedSlotIfValid(int newSelectedSlot)
    {
        if (newSelectedSlot < 0 || newSelectedSlot >= inventorySlots.Length)
            return;

        ChangeSelectedSlot(newSelectedSlot);
    }

    private void ChangeSelectedSlot(int newSelectedSlot)
    {
        if (inventorySlots == null || inventorySlots.Length == 0)
            return;

        if (newSelectedSlot < 0 || newSelectedSlot >= inventorySlots.Length)
            return;

        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
        {
            InventorySlot previous = inventorySlots[selectedSlot];
            if (previous != null)
                previous.Deselect();
        }

        InventorySlot next = inventorySlots[newSelectedSlot];
        if (next != null)
            next.Select();

        selectedSlot = newSelectedSlot;
        RefreshHeldItem();
    }

    public void SelectSlot(int slotIndex)
    {
        ChangeSelectedSlotIfValid(slotIndex);
    }

    public bool AddItem(Item item)
    {
        if (item == null)
            return false;

        // Loot always goes into the grid if possible
        if (item.type == ItemType.Loot)
        {
            if (gridLootInventory == null)
            {
                Debug.LogWarning($"{nameof(InventoryManager)}: gridLootInventory is not assigned.", this);
                ShowNoSpaceMessage();
                return false;
            }

            bool addedToGrid = gridLootInventory.TryAddItem(item);
            if (!addedToGrid)
                ShowNoSpaceMessage();

            return addedToGrid;
        }

        // Non-loot goes into hotbar slots
        if (inventorySlots == null || inventorySlots.Length == 0)
            return false;

        if (inventoryItemPrefab == null)
        {
            Debug.LogWarning($"{nameof(InventoryManager)}: inventoryItemPrefab is not assigned.", this);
            return false;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            if (slot == null)
                continue;

            if (GetDraggableItemInSlot(slot) == null)
            {
                SpawnNewItem(item, slot);
                ChangeSelectedSlot(i);
                return true;
            }
        }

        // Hotbar full
        ShowNoSpaceMessage();
        return false;
    }

    private void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItemObject = Instantiate(inventoryItemPrefab, slot.transform);
        newItemObject.transform.SetAsFirstSibling();

        RectTransform rectTransform = newItemObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.sizeDelta = itemUISize;
        }

        DraggableItem draggableItem = newItemObject.GetComponent<DraggableItem>();
        if (draggableItem != null)
        {
            draggableItem.InitialiseItem(item);
            draggableItem.inventoryManager = this;
        }
        else
        {
            Debug.LogWarning($"{nameof(InventoryManager)}: Spawned prefab has no DraggableItem component.", newItemObject);
        }
    }

    private DraggableItem GetDraggableItemInSlot(InventorySlot slot)
    {
        return slot.GetComponentInChildren<DraggableItem>();
    }

    public void RefreshHeldItem()
    {
        if (playerHoldItem == null)
            return;

        if (inventorySlots == null || inventorySlots.Length == 0)
        {
            playerHoldItem.ClearHeldItem();
            return;
        }

        if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length)
        {
            playerHoldItem.ClearHeldItem();
            return;
        }

        InventorySlot slot = inventorySlots[selectedSlot];
        if (slot == null)
        {
            playerHoldItem.ClearHeldItem();
            return;
        }

        DraggableItem draggableItem = GetDraggableItemInSlot(slot);

        if (draggableItem != null &&
            draggableItem.item != null &&
            draggableItem.item.heldPrefab != null)
        {
            playerHoldItem.HoldItem(draggableItem.item.heldPrefab);
        }
        else
        {
            playerHoldItem.ClearHeldItem();
        }
    }
}
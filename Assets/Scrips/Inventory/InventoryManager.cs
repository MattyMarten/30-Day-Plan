using UnityEngine;
using StarterAssets;

public class InventoryManager : MonoBehaviour
{
    [Header("Inventory")]
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;

    [Header("References")]
    public StarterAssetsInputs input;
    public PlayerHoldItem playerHoldItem;

    [Header("UI")]
    public Vector2 itemUISize = new Vector2(80, 80);

    private int selectedSlot = -1;

    private void Start()
    {
        if (playerHoldItem == null)
            playerHoldItem = FindAnyObjectByType<PlayerHoldItem>();

        if (input == null)
            input = FindAnyObjectByType<StarterAssetsInputs>();

        if (inventorySlots != null && inventorySlots.Length > 0)
        {
            ChangeSelectedSlot(0);
        }
    }

    private void Update()
    {
        if (input == null || inventorySlots == null || inventorySlots.Length == 0)
            return;

        HandleNumberInput();
        HandleScrollInput();
    }

    private void HandleNumberInput()
    {
        if (input.Slot1) { SelectSlotAndReset(0, ref input.Slot1); }
        else if (input.Slot2) { SelectSlotAndReset(1, ref input.Slot2); }
        else if (input.Slot3) { SelectSlotAndReset(2, ref input.Slot3); }
        else if (input.Slot4) { SelectSlotAndReset(3, ref input.Slot4); }
        else if (input.Slot5) { SelectSlotAndReset(4, ref input.Slot5); }
        else if (input.Slot6) { SelectSlotAndReset(5, ref input.Slot6); }
        else if (input.Slot7) { SelectSlotAndReset(6, ref input.Slot7); }
        else if (input.Slot8) { SelectSlotAndReset(7, ref input.Slot8); }
        else if (input.Slot9) { SelectSlotAndReset(8, ref input.Slot9); }
        else if (input.Slot0) { SelectSlotAndReset(9, ref input.Slot0); }
    }

    private void HandleScrollInput()
    {
        if (input.scroll == 0)
            return;

        int direction = input.scroll > 0 ? 1 : -1;
        int newSlot = selectedSlot + direction;

        if (newSlot >= inventorySlots.Length)
            newSlot = 0;
        else if (newSlot < 0)
            newSlot = inventorySlots.Length - 1;

        ChangeSelectedSlot(newSlot);
        input.scroll = 0;
    }

    private void SelectSlotAndReset(int slotIndex, ref bool inputFlag)
    {
        ChangeSelectedSlot(slotIndex);
        inputFlag = false;
    }

    private void ChangeSelectedSlot(int newSelectedSlot)
    {
        if (newSelectedSlot < 0 || newSelectedSlot >= inventorySlots.Length)
            return;

        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length)
        {
            inventorySlots[selectedSlot].Deselect();
        }

        inventorySlots[newSelectedSlot].Select();
        selectedSlot = newSelectedSlot;

        RefreshHeldItem();
    }

    public bool AddItem(Item item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];

            if (slot == null)
                continue;

            if (GetDraggableItemInSlot(slot) == null)
            {
                SpawnNewItem(item, slot);
                ChangeSelectedSlot(i);
                if (i == selectedSlot)
                {
                    RefreshHeldItem();
                }

                return true;
            }
        }

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

        if (selectedSlot < 0 || selectedSlot >= inventorySlots.Length)
        {
            playerHoldItem.ClearHeldItem();
            return;
        }

        InventorySlot slot = inventorySlots[selectedSlot];
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
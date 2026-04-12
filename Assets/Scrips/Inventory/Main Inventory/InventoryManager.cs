using UnityEngine;
using StarterAssets;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    // =========================
    // Inspector fields
    // =========================

    [Header("Inventory")]
    [SerializeField] private InventorySlot[] inventorySlots;
    [SerializeField] private GameObject inventoryItemPrefab;

    [Header("Toolbar")]
    [SerializeField, Min(1)] private int activeSlotCount = 5;

    [Header("Toolbar UI Sizing")]
    [SerializeField] private RectTransform toolbarRect;
    [SerializeField] private float toolbarBaseWidth = 100f;   // width for 1 slot
    [SerializeField] private float toolbarStepWidth = 90f;    // extra width per additional slot

    [Header("UtilityItemView (selected item icon preview)")]
    [SerializeField] private GameObject utilityItemView;
    [SerializeField] private Image utilityItemViewIcon;
    [SerializeField, Min(0f)] private float utilityItemViewHideDelay = 5f;

    [Header("Loot Grid Inventory (assign in inspector)")]
    [SerializeField] private GridInventory gridLootInventory;

    [Header("References (assign in inspector)")]
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private PlayerHoldItem playerHoldItem;

    [Header("UI")]
    [SerializeField] private Vector2 itemUISize = new Vector2(80, 80);
    [SerializeField] private RectTransform dragLayer;

    [Header("Messages")]
    [Tooltip("A GameObject that contains your NO Space text (setActive true/false).")]
    [SerializeField] private GameObject noSpaceMessage;
    [SerializeField] private float noSpaceMessageSeconds = 1.5f;

    // =========================
    // Runtime state
    // =========================

    private float noSpaceHideAt;
    private int selectedSlot = -1;

    private float utilityItemViewHideAt = -1f;

    // =========================
    // Unity lifecycle
    // =========================

    private void Awake()
    {
        if (noSpaceMessage != null)
            noSpaceMessage.SetActive(false);

        if (utilityItemView != null)
            utilityItemView.SetActive(false);

        // Optional auto-find if I forgot to assign it
        if (utilityItemViewIcon == null && utilityItemView != null)
            utilityItemViewIcon = utilityItemView.GetComponentInChildren<Image>(true);
    }

    private void Start()
    {
        if (playerHoldItem == null)
            playerHoldItem = FindAnyObjectByType<PlayerHoldItem>();

        if (input == null)
            input = FindAnyObjectByType<StarterAssetsInputs>();

        ApplyActiveSlotsToUI();

        if (ActiveCount > 0)
            ChangeSelectedSlot(0);
    }

    private void Update()
    {
        // Hide "no space" message after timeout
        if (noSpaceMessage != null && noSpaceMessage.activeSelf && Time.time >= noSpaceHideAt)
            noSpaceMessage.SetActive(false);

        // Hide UtilityItemView after timeout
        TryAutoHideUtilityItemView();

        if (inventorySlots == null || inventorySlots.Length == 0)
            return;

        if (input == null)
            return;

        HandleNumberInput();
        HandleScrollInput();
    }

    public void SelectSlot(int slotIndex)
    {
        ChangeSelectedSlotIfValid(slotIndex);
    }

    public void SetActiveSlotCount(int newCount)
    {
        activeSlotCount = newCount;
        ApplyActiveSlotsToUI();

        if (ActiveCount > 0 && (selectedSlot < 0 || selectedSlot >= ActiveCount))
            ChangeSelectedSlot(0);
    }

    public bool AddItem(Item item)
    {
        if (item == null)
            return false;

        // Loot always goes into the grid
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
        int count = ActiveCount;
        if (count <= 0)
            return false;

        for (int i = 0; i < count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            if (slot == null)
                continue;

            if (GetDraggableItemInSlot(slot) == null)
            {
                SpawnNewItem(item, slot);
                ChangeSelectedSlot(i); // also refreshes icon preview + held item
                return true;
            }
        }

        ShowNoSpaceMessage();
        return false;
    }

    public void RefreshHeldItem()
    {
        if (playerHoldItem == null)
            return;

        int count = ActiveCount;
        if (inventorySlots == null || inventorySlots.Length == 0 || count <= 0)
        {
            playerHoldItem.ClearHeldItem();
            return;
        }

        if (selectedSlot < 0 || selectedSlot >= count)
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

    // =========================
    // Input handling
    // =========================

    private void HandleNumberInput()
    {
        int count = ActiveCount;
        if (count <= 0) return;

        if (input.Slot1) { ChangeSelectedSlotIfValid(0); input.Slot1 = false; }
        else if (input.Slot2) { ChangeSelectedSlotIfValid(1); input.Slot2 = false; }
        else if (input.Slot3) { ChangeSelectedSlotIfValid(2); input.Slot3 = false; }
        else if (input.Slot4) { ChangeSelectedSlotIfValid(3); input.Slot4 = false; }
        else if (input.Slot5) { ChangeSelectedSlotIfValid(4); input.Slot5 = false; }
        else if (input.Slot6) { ChangeSelectedSlotIfValid(5); input.Slot6 = false; }
        else if (input.Slot7) { ChangeSelectedSlotIfValid(6); input.Slot7 = false; }
        else if (input.Slot8) { ChangeSelectedSlotIfValid(7); input.Slot8 = false; }
        else if (input.Slot9) { ChangeSelectedSlotIfValid(8); input.Slot9 = false; }
        else if (input.Slot0) { ChangeSelectedSlotIfValid(9); input.Slot0 = false; }
    }

    private void HandleScrollInput()
    {
        if (input.scroll == 0f)
            return;

        int count = ActiveCount;
        if (count <= 0) return;

        int direction = input.scroll > 0 ? 1 : -1;
        int newSlot = selectedSlot + direction;

        if (newSlot >= count)
            newSlot = 0;
        else if (newSlot < 0)
            newSlot = count - 1;

        ChangeSelectedSlot(newSlot);
        input.scroll = 0f;
    }

    // =========================
    // Slot selection
    // =========================

    private void ChangeSelectedSlotIfValid(int newSelectedSlot)
    {
        int count = ActiveCount;
        if (count <= 0) return;

        if (newSelectedSlot < 0 || newSelectedSlot >= count)
            return;

        ChangeSelectedSlot(newSelectedSlot);
    }

    private void ChangeSelectedSlot(int newSelectedSlot)
    {
        int count = ActiveCount;
        if (count <= 0) return;

        if (newSelectedSlot < 0 || newSelectedSlot >= count)
            return;

        if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length && inventorySlots[selectedSlot] != null)
            inventorySlots[selectedSlot].Deselect();

        if (inventorySlots[newSelectedSlot] != null)
            inventorySlots[newSelectedSlot].Select();

        selectedSlot = newSelectedSlot;

        RefreshHeldItem();
        ShowUtilityItemViewTemporarily();
        UpdateUtilityItemViewIcon();
    }

    // =========================
    // Active slot count / toolbar sizing
    // =========================

    private int ActiveCount
    {
        get
        {
            if (inventorySlots == null || inventorySlots.Length == 0)
                return 0;

            return Mathf.Clamp(activeSlotCount, 1, inventorySlots.Length);
        }
    }

    private void ApplyActiveSlotsToUI()
    {
        if (inventorySlots == null)
            return;

        int count = ActiveCount;

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] == null) continue;
            inventorySlots[i].gameObject.SetActive(i < count);
        }

        // Keep selection valid
        if (count > 0 && selectedSlot >= count)
            selectedSlot = count - 1;

        ApplyToolbarSize();
    }

    private void ApplyToolbarSize()
    {
        if (toolbarRect == null)
            return;

        int count = ActiveCount;
        if (count <= 0)
            return;

        float width = toolbarBaseWidth + (count - 1) * toolbarStepWidth;

        Vector2 size = toolbarRect.sizeDelta;
        size.x = width;
        toolbarRect.sizeDelta = size;
    }

    // =========================
    // UtilityItemView icon preview
    // =========================

    private void ShowUtilityItemViewTemporarily()
    {
        if (utilityItemView == null)
            return;

        utilityItemView.SetActive(true);
        utilityItemViewHideAt = Time.time + utilityItemViewHideDelay;
    }

    private void TryAutoHideUtilityItemView()
    {
        if (utilityItemView == null)
            return;

        if (utilityItemView.activeSelf && utilityItemViewHideDelay > 0f && Time.time >= utilityItemViewHideAt)
            utilityItemView.SetActive(false);
    }

    private void UpdateUtilityItemViewIcon()
    {
        if (utilityItemViewIcon == null)
            return;

        Sprite sprite = null;

        int count = ActiveCount;
        if (count > 0 && selectedSlot >= 0 && selectedSlot < count)
        {
            InventorySlot slot = inventorySlots[selectedSlot];
            if (slot != null)
            {
                DraggableItem di = GetDraggableItemInSlot(slot);
                if (di != null && di.item != null)
                    sprite = di.item.image;
            }
        }

        utilityItemViewIcon.sprite = sprite;

        // Hide icon if empty selection
        utilityItemViewIcon.enabled = (sprite != null);
    }

    // =========================
    // Spawning / slot queries
    // =========================

    private void SpawnNewItem(Item item, InventorySlot slot)
    {
        if (inventoryItemPrefab == null)
        {
            Debug.LogWarning($"{nameof(InventoryManager)}: inventoryItemPrefab is not assigned.", this);
            return;
        }

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
            draggableItem.SetDragParent(dragLayer);
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
        if (slot == null) return null;

        // Only count items that are direct children of the slot
        for (int i = 0; i < slot.transform.childCount; i++)
        {
            Transform child = slot.transform.GetChild(i);
            DraggableItem di = child.GetComponent<DraggableItem>();
            if (di != null)
                return di;
        }

        return null;
    }

    // =========================
    // Messaging
    // =========================

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
}
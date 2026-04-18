using UnityEngine;
using StarterAssets;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayer;

    [Header("References")]
    [SerializeField] private StarterAssetsInputs input;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private PlayerHoldItem playerHoldItem;

    [Header("UI")]
    [SerializeField] private GameObject interactText;
    [SerializeField] private GameObject storageUIPanel;            // assign your UI Panel
    [SerializeField] private RawMaterialStorage storage;           // assign your storage chest

    [Header("Interact UI")]
    [SerializeField] private GameObject pickupInfoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    [SerializeField] private TMPro.TextMeshProUGUI itemRarityText;
    [SerializeField] private UnityEngine.UI.Image itemSpriteImage;
    [SerializeField] private UnityEngine.UI.Image rarityBackground;
    [SerializeField] private TMPro.TextMeshProUGUI materialValueText;
    [SerializeField] private GameObject grinderPromptPanel;
    [SerializeField] private TMPro.TextMeshProUGUI grinderPromptText;
    [SerializeField] private GameObject storagePromptPanel;      // assign "PRESS E TO OPEN STORAGE" panel (optional)
    [SerializeField] private TMPro.TextMeshProUGUI storagePromptText;
    [SerializeField] private GameObject craftingUIPanel;         // assign the Crafting Canvas panel here
    [SerializeField] private CraftingStationUI craftingUI;       // assign the CraftingStationUI script

    private void Awake()
    {
        if (input == null)
            input = GetComponent<StarterAssetsInputs>();
        if (playerHoldItem == null)
            playerHoldItem = GetComponent<PlayerHoldItem>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        HideAllUIPanels();
    }

    [System.Obsolete]
    private void Update()
    {
        if (!AreCoreRefsValid())
        {
            SetUI(false);
            return;
        }

        // Panels: hide after pressing interact
        if (CheckAndHandleOpenPanels()) return;

        // Perform spherecast for all detectable interactions
        bool hasTarget = Physics.SphereCast(
            new Ray(playerCamera.transform.position, playerCamera.transform.forward),
            0.4f,
            out RaycastHit hit,
            interactDistance,
            interactLayer,
            QueryTriggerInteraction.Collide
        );

        RawMaterialStorage storageChest = hasTarget ? hit.collider.GetComponentInParent<RawMaterialStorage>() : null;
        CraftingTable craftingTable = hasTarget ? hit.collider.GetComponentInParent<CraftingTable>() : null;
        GrinderMachine grinder = hasTarget ? hit.collider.GetComponentInParent<GrinderMachine>() : null;
        ItemPickup pickup = hasTarget ? hit.collider.GetComponentInParent<ItemPickup>() : null;

        // Storage logic
        HandleStorage(storageChest);

        // Crafting station logic
        if (HandleCraftingStation(craftingTable)) return;

        // Grinder logic
        HandleGrinder(grinder);

        // Pickup logic (incl. UI/prompt)
        HandleItemPickup(pickup);
    }

    private bool AreCoreRefsValid()
    {
        return !(input == null || playerCamera == null);
    }

    private void HideAllUIPanels()
    {
        if (interactText != null)          interactText.SetActive(false);
        if (pickupInfoPanel != null)       pickupInfoPanel.SetActive(false);
        if (storageUIPanel != null)        storageUIPanel.SetActive(false);
        if (storagePromptPanel != null)    storagePromptPanel.SetActive(false);
        if (grinderPromptPanel != null)    grinderPromptPanel.SetActive(false);
        if (craftingUIPanel != null)       craftingUIPanel.SetActive(false);
    }

    private bool CheckAndHandleOpenPanels()
    {
        // Hide storage
        if (storageUIPanel != null && storageUIPanel.activeSelf && input.ConsumeInteract())
        {
            storageUIPanel.SetActive(false);
            SetUIMode(false);
            return true;
        }
        // Hide crafting
        if (craftingUIPanel != null && craftingUIPanel.activeSelf && input.ConsumeInteract())
        {
            craftingUIPanel.SetActive(false);
            SetUIMode(false);
            return true;
        }
        return false;
    }

    private void HandleStorage(RawMaterialStorage storageChest)
    {
        bool lookingAtStorage = storageChest != null;

        if (storagePromptText != null)
            storagePromptText.text = lookingAtStorage ? "OPEN STORAGE" : "";

        if (lookingAtStorage && input.ConsumeInteract())
        {
            if (storageUIPanel != null) storageUIPanel.SetActive(true);
            SetUIMode(true);

            var storageUI = storageUIPanel?.GetComponent<MaterialStorageUI>();
            if (storageUI != null)
            {
                storageUI.storage = storageChest.GetComponent<RawMaterialStorage>();
                storageUI.RefreshUI();
            }
        }
    }

    private bool HandleCraftingStation(CraftingTable craftingTable)
    {
        bool lookingAtCrafting = craftingTable != null;
        if (lookingAtCrafting && input.ConsumeInteract())
        {
            if (craftingUIPanel != null) craftingUIPanel.SetActive(true);
            SetUIMode(true);

            if (craftingUI != null) craftingUI.RefreshUI();
            return true; // End update so no double-input
        }
        return false;
    }

    private void HandleGrinder(GrinderMachine grinder)
    {
        bool lookingAtGrinder = grinder != null;

        if (grinderPromptPanel != null)
            grinderPromptPanel.SetActive(lookingAtGrinder);

        if (grinderPromptText != null)
            grinderPromptText.text = lookingAtGrinder ? "GRIND" : "";

        if (lookingAtGrinder && input.ConsumeInteract())
        {
            grinder.Grind();
            if (grinderPromptPanel != null)
                grinderPromptPanel.SetActive(false); // Hide after grinding
        }
    }

    private void HandleItemPickup(ItemPickup pickup)
    {
        bool lookingAtPickup = pickup != null;

        if (interactText != null)
            interactText.SetActive(lookingAtPickup);

        if (pickupInfoPanel != null)
            pickupInfoPanel.SetActive(lookingAtPickup);

        if (!lookingAtPickup) return;

        // Update info
        if (pickupInfoPanel != null && pickup.Item != null)
        {
            string htmlColor = ColorUtility.ToHtmlStringRGB(pickup.Item.RarityColor);
            string infoText =
            $"<b>{pickup.Item.itemName}</b>\n" +
            $"<size=80%><color=#{htmlColor}>{pickup.Item.rarity}</color></size>\n" +
            $"{FormatMaterialValue(pickup.Item)}";
            itemNameText.text = infoText;
            itemSpriteImage.sprite = pickup.Item.image;
            rarityBackground.color = pickup.Item.RarityColor;
        }

        // Handle pickup
        if (input.ConsumeInteract())
        {
            if (inventoryManager == null)
            {
                Debug.LogWarning($"{nameof(PlayerInteraction)}: inventoryManager is not assigned in the Inspector.", this);
                return;
            }
            pickup.PickUp(inventoryManager, playerHoldItem);
        }
    }

    private void SetUI(bool status)
    {
        if (interactText != null) interactText.SetActive(status);
        if (pickupInfoPanel != null) pickupInfoPanel.SetActive(status);
    }

    private string FormatMaterialValue(Item item)
    {
        if (item.MaterialValue == null || item.MaterialValue.Count == 0)
            return "No materials";
        return string.Join(", ", item.MaterialValue.Select(kv => $"{kv.Value}x {kv.Key.displayName}"));
    }

    private void SetUIMode(bool enabled)
    {
        // Disable/enable movement & look (FirstPersonController)
        var fps = GetComponent<StarterAssets.FirstPersonController>();
        if (fps != null)
            fps.enabled = !enabled;

        // Show/hide mouse
        Cursor.visible = enabled;
        Cursor.lockState = enabled ? CursorLockMode.None : CursorLockMode.Locked;
    }

}
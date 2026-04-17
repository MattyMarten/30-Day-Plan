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
    [SerializeField] private GameObject storageUIPanel;          // assign your UI Panel
    [SerializeField] private RawMaterialStorage storage;         // assign your storage chest


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
    [SerializeField] private GameObject craftingUIPanel;       // assign the Crafting Canvas panel here
    [SerializeField] private CraftingStationUI craftingUI;     // assign the CraftingStationUI script

    private void Awake()
    {
        if (input == null)
            input = GetComponent<StarterAssetsInputs>();

        if (playerHoldItem == null)
            playerHoldItem = GetComponent<PlayerHoldItem>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactText != null)              interactText.SetActive(false);
        if (pickupInfoPanel != null)           pickupInfoPanel.SetActive(false);
        if (storageUIPanel != null)            storageUIPanel.SetActive(false);
        if (storagePromptPanel != null)        storagePromptPanel.SetActive(false);
        if (grinderPromptPanel != null)        grinderPromptPanel.SetActive(false);
        if (craftingUIPanel != null)           craftingUIPanel.SetActive(false);
    }

    [System.Obsolete]
    private void Update()
    {
        if (input == null || playerCamera == null)
        {
            SetUI(false);
            return;
        }

        if (storageUIPanel != null && storageUIPanel.activeSelf)
        {
            if (input.ConsumeInteract())
            {
                storageUIPanel.SetActive(false);
                return; // Stop further input processing this frame
            }
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        float sphereRadius = 0.4f;

        bool hasTarget = Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide);

        RawMaterialStorage storageChest = hasTarget ? hit.collider.GetComponentInParent<RawMaterialStorage>() : null;

        bool lookingAtStorage = storageChest != null;

        if (storagePromptText != null)
            storagePromptText.text = lookingAtStorage ? "OPEN STORAGE" : "";

        if (lookingAtStorage && input.ConsumeInteract())
        {
            if (storageUIPanel != null) storageUIPanel.SetActive(true);

            var storageUI = storageUIPanel.GetComponent<MaterialStorageUI>();
            if (storageUI != null)
            {
                storageUI.storage = storageChest.GetComponent<RawMaterialStorage>();
                storageUI.RefreshUI();
            }
        }

        CraftingTable craftingTable = hasTarget ? hit.collider.GetComponentInParent<CraftingTable>() : null;

        bool lookingAtCrafting = craftingTable != null;

        if (craftingUIPanel != null && craftingUIPanel.activeSelf)
        {
            if (input.ConsumeInteract())
            {
                craftingUIPanel.SetActive(false);
                return;
            }
        }

        if (lookingAtCrafting && input.ConsumeInteract())
        {
            if (craftingUIPanel != null) craftingUIPanel.SetActive(true);
            if (craftingUI != null) craftingUI.RefreshUI();
            return; // Avoid accidental double-input in the same frame
        }



        GrinderMachine grinder = hasTarget ? hit.collider.GetComponentInParent<GrinderMachine>() : null;

        bool lookingAtGrinder = grinder != null;

        if (grinderPromptPanel != null)
            grinderPromptPanel.SetActive(lookingAtGrinder);

        if (grinderPromptText != null)
            grinderPromptText.text = lookingAtGrinder ? "GRIND" : "";

        if (lookingAtGrinder)
        {
            if (input.ConsumeInteract())
            {
                grinder.GrindAllItems();
                grinderPromptPanel.SetActive(false); // Hide after grinding
            }
        }


        ItemPickup pickup = hasTarget ? hit.collider.GetComponentInParent<ItemPickup>() : null;

        bool lookingAtPickup = pickup != null;

        // UI Prompt logic
        if (interactText != null)
            interactText.SetActive(lookingAtPickup);

        // Pickup info panel logic
        if (pickupInfoPanel != null)
            pickupInfoPanel.SetActive(lookingAtPickup);

        // If not looking at a pickup, we're done for this frame
        if (!lookingAtPickup)
            return;

        // Update the info panel if active
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

        // Handle the interact input for pickup
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
        if (interactText != null)
            interactText.SetActive(status);
        if (pickupInfoPanel != null)
            pickupInfoPanel.SetActive(status);
    }

    // Example for materials
    private string FormatMaterialValue(Item item)
    {
        if (item.MaterialValue == null || item.MaterialValue.Count == 0)
            return "No materials";
        return string.Join(", ", item.MaterialValue.Select(kv => $"{kv.Value}x {kv.Key}"));
    }
}
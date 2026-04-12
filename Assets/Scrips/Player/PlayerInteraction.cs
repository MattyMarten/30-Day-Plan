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

    [Header("Interact UI")]
    [SerializeField] private GameObject pickupInfoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI itemNameText;
    [SerializeField] private TMPro.TextMeshProUGUI itemRarityText;
    [SerializeField] private UnityEngine.UI.Image itemSpriteImage;
    [SerializeField] private UnityEngine.UI.Image rarityBackground;
    [SerializeField] private TMPro.TextMeshProUGUI materialValueText;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<StarterAssetsInputs>();

        if (playerHoldItem == null)
            playerHoldItem = GetComponent<PlayerHoldItem>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (interactText != null)
            interactText.SetActive(false);
        if (pickupInfoPanel != null)
            pickupInfoPanel.SetActive(false);
    }

    private void Update()
    {
        if (input == null || playerCamera == null)
        {
            SetUI(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        float sphereRadius = 0.4f;

        bool hasTarget = Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Collide);

        GrinderMachine grinder = hasTarget ? hit.collider.GetComponentInParent<GrinderMachine>() : null;

        if (grinder != null)
        {
            // Show a "Press [E] to Grind All" prompt
            interactText.SetActive(true);

            // Check for interact input (E or other)
            if (input.ConsumeInteract()) // or your Input System action for interact
            {
                grinder.GrindAllItems();
            }
        }
        else
        {
            interactText.SetActive(false);
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
            itemNameText.text = pickup.Item.itemName + "\n" + FormatMaterialValue(pickup.Item);
            string htmlColor = ColorUtility.ToHtmlStringRGB(pickup.Item.RarityColor);
            itemRarityText.text = $"<color=#{htmlColor}>{pickup.Item.rarity}</color>";
            itemSpriteImage.sprite = pickup.Item.image;
            rarityBackground.color = pickup.Item.RarityColor; // uses your new property!
            
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
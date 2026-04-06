using UnityEngine;
using StarterAssets;

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
    }

    private void Update()
    {
        if (input == null || playerCamera == null)
        {
            if (interactText != null) interactText.SetActive(false);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        bool hasTarget = Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer, QueryTriggerInteraction.Ignore);

        if (!hasTarget)
        {
            if (interactText != null) interactText.SetActive(false);
            return;
        }

        ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();
        if (pickup == null)
        {
            if (interactText != null) interactText.SetActive(false);
            return;
        }

        if (interactText != null) interactText.SetActive(true);

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
}
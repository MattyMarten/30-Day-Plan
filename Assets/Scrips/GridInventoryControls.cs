using UnityEngine;
using StarterAssets;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GridInventoryControls : MonoBehaviour
{
    [Header("Optional References (auto-found on same GameObject if missing)")]
    [SerializeField] private StarterAssetsInputs input;

    [Header("Grid Inventory Interaction")]
    [SerializeField] private bool enableGridInteraction = true;

    [Header("Grid Item Rotation")]
    [SerializeField] private bool allowRotation = true;

    // Hovered grid (set by GridInteract)
    private GridInventory selectedGrid;

    // Held item
    private InventoryLoot heldItem;
    private RectTransform heldItemRect;

    // Where the held item came from (for returning)
    private GridInventory originGrid;
    private Vector2Int originTopLeft;
    private bool hasOrigin;

    // UI open flag (set by InventoryControls)
    private bool uiOpen;

    // Placement preview caching
    private Vector2Int lastPreviewTopLeft;
    private bool hasPreview;

    public void SetUIOpen(bool open)
    {
        uiOpen = open;

        if (!uiOpen)
        {
            ReturnHeldItemToOrigin(); // prevents “stuck on mouse”
            ClearPreview();
        }
    }

    public void SetRotationAllowed(bool allowed) => allowRotation = allowed;

    public void SetSelectedGrid(GridInventory grid)
    {
        if (selectedGrid == grid)
            return;

        // Clear preview on old grid when switching hover
        ClearPreview();

        selectedGrid = grid;
        hasPreview = false;
    }

    private void Awake()
    {
        if (input == null)
            input = GetComponent<StarterAssetsInputs>();
    }

    private void Update()
    {
        if (!uiOpen || !enableGridInteraction)
            return;

        if (input != null && input.ConsumeRandomLoot() && selectedGrid != null)
            selectedGrid.TrySpawnRandomLootItem();

        UpdateDragFollow();
        UpdatePlacementPreview();

        if (allowRotation && heldItem != null && WasRotateKeyThisFrame())
        {
            heldItem.RotateClockwise();
            heldItemRect = heldItem.GetComponent<RectTransform>();
            hasPreview = false;
            return;
        }

        if (WasLeftClickThisFrame())
            HandleLeftClick();
    }

    private void UpdateDragFollow()
    {
        if (heldItemRect == null)
            return;

        Vector2 p = GetMouseScreenPosition();
        heldItemRect.position = new Vector2(Mathf.Round(p.x), Mathf.Round(p.y));
        heldItemRect.SetAsLastSibling();
    }

    private void UpdatePlacementPreview()
    {
        if (heldItem == null)
        {
            if (selectedGrid != null)
                selectedGrid.ClearPlacementPreview();
            hasPreview = false;
            return;
        }

        // Only show preview if we are actually hovering a grid AND the mouse is inside its rect
        if (selectedGrid == null)
            return;

        Vector2 mousePos = GetMouseScreenPosition();

        if (!selectedGrid.TryGetTile(mousePos, out Vector2Int hoveredTile))
        {
            selectedGrid.ClearPlacementPreview();
            hasPreview = false;
            return;
        }

        Vector2Int topLeft = selectedGrid.GetTopLeftForCenteredPlacement(hoveredTile, heldItem);

        if (!hasPreview || topLeft != lastPreviewTopLeft)
        {
            selectedGrid.ShowPlacementPreview(heldItem, topLeft.x, topLeft.y);
            lastPreviewTopLeft = topLeft;
            hasPreview = true;
        }
    }

    private void HandleLeftClick()
    {
        Vector2 mousePos = GetMouseScreenPosition();

        // If holding item and click is NOT over a valid tile -> cancel (return to origin)
        if (heldItem != null)
        {
            if (selectedGrid == null || !selectedGrid.TryGetTile(mousePos, out Vector2Int hoveredTile))
            {
                ReturnHeldItemToOrigin();
                return;
            }

            // Attempt placement
            Vector2Int topLeft = selectedGrid.GetTopLeftForCenteredPlacement(hoveredTile, heldItem);

            if (selectedGrid.TryPlaceItem(heldItem, topLeft.x, topLeft.y))
            {
                heldItem = null;
                heldItemRect = null;
                originGrid = null;
                hasOrigin = false;

                selectedGrid.ClearPlacementPreview();
                hasPreview = false;
            }

            return;
        }

        // Not holding anything -> attempt pickup (must be on a grid tile)
        if (selectedGrid == null || !selectedGrid.TryGetTile(mousePos, out Vector2Int pickTile))
            return;

        // Determine item + its top-left (so we can return it on cancel)
        if (!selectedGrid.TryFindItemTopLeftAt(pickTile, out InventoryLoot found, out Vector2Int foundTopLeft))
            return;

        InventoryLoot picked = selectedGrid.PickUpLoot(pickTile.x, pickTile.y);
        if (picked == null)
            return;

        heldItem = picked;
        heldItemRect = heldItem.GetComponent<RectTransform>();
        heldItemRect.SetAsLastSibling();

        originGrid = selectedGrid;
        originTopLeft = foundTopLeft;
        hasOrigin = true;

        hasPreview = false;
    }

    private void ReturnHeldItemToOrigin()
    {
        if (heldItem == null)
            return;

        // Clear preview from current hover grid
        ClearPreview();

        if (hasOrigin && originGrid != null)
        {
            // Best effort: put it back
            originGrid.TryPlaceItem(heldItem, originTopLeft.x, originTopLeft.y);
        }

        heldItem = null;
        heldItemRect = null;
        originGrid = null;
        hasOrigin = false;
        hasPreview = false;
    }

    private void ClearPreview()
    {
        if (selectedGrid != null)
            selectedGrid.ClearPlacementPreview();
        hasPreview = false;
    }

    private static Vector2 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private static bool WasLeftClickThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private static bool WasRotateKeyThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.T);
#endif
    }
}
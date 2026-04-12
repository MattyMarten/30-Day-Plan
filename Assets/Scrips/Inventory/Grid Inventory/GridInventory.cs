using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GridInventory : MonoBehaviour
{
    private const float tileSizeWidth = 64f;
    private const float tileSizeHeight = 64f;

    private InventoryLoot[,] inventoryLootSlot;
    private RectTransform rectTransform;

    [SerializeField] private int gridSizeWidth = 10;
    [SerializeField] private int gridSizeHeight = 8;

    [Header("Prefab + Possible Items")]
    [SerializeField] private InventoryLoot inventoryLootPrefab;
    [SerializeField] private Item[] possibleItems;

    [Header("Placement Preview")]
    [SerializeField] private RectTransform rarityHighlightRoot;
    [SerializeField] private RectTransform placementHighlightRoot;
    [SerializeField] private Image rarityTilePrefab;
    [SerializeField] private Image placementTilePrefab;
    [SerializeField] private Image highlightTilePrefab;     // 64x64 image prefab
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.25f); //green
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.25f); //red
    [SerializeField] private Color swapColor = new Color(1f, 0.5f, 0f, 0.25f); //orange

    [Header("Rarity Tile Colors")]
    [SerializeField] private Color commonTileColor = new Color(0.45f, 0.45f, 0.45f, 0.25f); // gray
    [SerializeField] private Color uncommonTileColor = new Color(0.20f, 0.45f, 1.00f, 0.25f); // blue
    [SerializeField] private Color rareTileColor = new Color(1.00f, 0.35f, 0.75f, 0.25f); // pink-ish
    [SerializeField] private Color legendaryTileColor = new Color(1.00f, 0.80f, 0.15f, 0.25f); // gold
    [SerializeField] private Color cursedTileColor = new Color(0.60f, 0.20f, 0.85f, 0.25f); // purple

    private readonly List<Image> rarityPool = new();
    private int rarityUsed;

    private readonly List<Image> placementPool = new();
    private int placementUsed;

    private int hoverX = -1, hoverY = -1;
    public void SetHoverTile(int x, int y) { hoverX = x; hoverY = y; }
    public void ClearHoverTile() { hoverX = hoverY = -1; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Init(gridSizeWidth, gridSizeHeight);
    }

    private void Init(int width, int height)
    {
        inventoryLootSlot = new InventoryLoot[width, height];
        rectTransform.sizeDelta = new Vector2(width * tileSizeWidth, height * tileSizeHeight);
    }

    private bool IsInBounds(int x, int y)
    {
        return inventoryLootSlot != null &&
               x >= 0 && y >= 0 &&
               x < inventoryLootSlot.GetLength(0) &&
               y < inventoryLootSlot.GetLength(1);
    }

    public bool TryGetTile(Vector2 screenMousePos, out Vector2Int tile)
    {
        tile = default;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, screenMousePos, null, out Vector2 local))
            return false;

        Rect r = rectTransform.rect;

        float xFromLeft = local.x - r.xMin;
        float yFromTop = r.yMax - local.y;

        int x = Mathf.FloorToInt(xFromLeft / tileSizeWidth);
        int y = Mathf.FloorToInt(yFromTop / tileSizeHeight);

        int maxX = Mathf.FloorToInt(r.width / tileSizeWidth);
        int maxY = Mathf.FloorToInt(r.height / tileSizeHeight);

        if (x < 0 || y < 0 || x >= maxX || y >= maxY)
            return false;

        tile = new Vector2Int(x, y);
        return true;
    }

    public InventoryLoot GetItemAt(int x, int y)
    {
        if (!IsInBounds(x, y)) return null;
        return inventoryLootSlot[x, y];
    }

    public IEnumerable<Vector2Int> GetFootprintCellsPublic(InventoryLoot loot)
    {
        return GetFootprintCells(loot);
    }

    public bool CanPlaceIgnoring(InventoryLoot item, int x, int y, InventoryLoot ignore)
    {
        if (item == null) return false;

        GetRotatedSize(item, out int w, out int h);

        if (!IsInBounds(x, y)) return false;
        if (!IsInBounds(x + w - 1, y + h - 1)) return false;

        foreach (var cell in GetFootprintCells(item))
        {
            int gx = x + cell.x;
            int gy = y + cell.y;

            if (!IsInBounds(gx, gy))
                return false;

            InventoryLoot at = inventoryLootSlot[gx, gy];
            if (at != null && at != ignore)
                return false;
        }

        return true;
    }

    // ---- Rotation helpers ----

    private static void GetBaseSize(InventoryLoot loot, out int baseW, out int baseH)
    {
        if (loot != null && loot.item != null)
        {
            baseW = Mathf.Max(1, loot.item.sizeWidth);
            baseH = Mathf.Max(1, loot.item.sizeHeight);
        }
        else
        {
            baseW = Mathf.Max(1, loot != null ? loot.sizeWidth : 1);
            baseH = Mathf.Max(1, loot != null ? loot.sizeHeight : 1);
        }
    }

    private void GetRotatedSize(InventoryLoot item, out int w, out int h)
    {
        GetBaseSize(item, out int baseW, out int baseH);

        bool swap = item.rotation == LootRotation.R90;
        w = swap ? baseH : baseW;
        h = swap ? baseW : baseH;
    }

    private Vector2Int RotateCell(Vector2Int c, int baseW, int baseH, LootRotation rot)
    {
        return rot == LootRotation.R90
            ? new Vector2Int(baseH - 1 - c.y, c.x)
            : c;
    }

    // ---- Footprint helpers ----

    private bool HasCustomFootprint(InventoryLoot loot)
    {
        return loot != null &&
               loot.item != null &&
               loot.item.occupiedCells != null &&
               loot.item.occupiedCells.Length > 0;
    }

    private IEnumerable<Vector2Int> GetFootprintCells(InventoryLoot loot)
    {
        GetBaseSize(loot, out int baseW, out int baseH);

        if (!HasCustomFootprint(loot))
        {
            for (int ix = 0; ix < baseW; ix++)
            for (int iy = 0; iy < baseH; iy++)
                yield return RotateCell(new Vector2Int(ix, iy), baseW, baseH, loot.rotation);

            yield break;
        }

        foreach (var c in loot.item.occupiedCells)
        yield return RotateCell(c, baseW, baseH, loot.rotation);
    }

    // ---- Placement ----

    private bool Fits(InventoryLoot item, int x, int y)
    {
        if (item == null) return false;

        GetRotatedSize(item, out int w, out int h);

        if (!IsInBounds(x, y)) return false;
        if (!IsInBounds(x + w - 1, y + h - 1)) return false;

        foreach (var cell in GetFootprintCells(item))
        {
            int gx = x + cell.x;
            int gy = y + cell.y;

            if (!IsInBounds(gx, gy))
                return false;

            if (inventoryLootSlot[gx, gy] != null)
                return false;
        }

        return true;
    }

    private void Occupy(InventoryLoot item, int x, int y)
    {
        GetRotatedSize(item, out int w, out int h);

        foreach (var cell in GetFootprintCells(item))
            inventoryLootSlot[x + cell.x, y + cell.y] = item;

        RectTransform lootRect = item.GetComponent<RectTransform>();
        lootRect.SetParent(rectTransform);

        float pxW = w * tileSizeWidth;
        float pxH = h * tileSizeHeight;

        lootRect.localPosition = new Vector2(
            x * tileSizeWidth + pxW / 2f,
            -(y * tileSizeHeight + pxH / 2f)
        );

        Vector2 lp = lootRect.localPosition;
        lootRect.localPosition = new Vector2(Mathf.Round(lp.x), Mathf.Round(lp.y));
    }

    public bool TryPlaceItem(InventoryLoot item, int x, int y)
    {
        if (item == null) return false;
        if (!Fits(item, x, y)) return false;

        Occupy(item, x, y);
        return true;
    }

    public InventoryLoot PickUpLoot(int x, int y)
    {
        if (!IsInBounds(x, y)) return null;

        InventoryLoot item = inventoryLootSlot[x, y];
        if (item == null) return null;

        for (int ix = 0; ix < inventoryLootSlot.GetLength(0); ix++)
        for (int iy = 0; iy < inventoryLootSlot.GetLength(1); iy++)
            if (inventoryLootSlot[ix, iy] == item)
                inventoryLootSlot[ix, iy] = null;

        return item;
    }

    private bool TryGetSingleOverlappedItem(InventoryLoot placingItem, int topLeftX, int topLeftY, out InventoryLoot overlapped)
    {
        overlapped = null;
        if (placingItem == null) return false;

        foreach (var cell in GetFootprintCells(placingItem))
        {
            int gx = topLeftX + cell.x;
            int gy = topLeftY + cell.y;

            if (!IsInBounds(gx, gy))
                return false; // out of bounds -> treat as not swappable

            InventoryLoot at = inventoryLootSlot[gx, gy];
            if (at == null)
                continue;

            if (overlapped == null) overlapped = at;
            else if (overlapped != at)
            {
                overlapped = null; // overlaps multiple items
                return false;
            }
        }

        return overlapped != null;
    }

    public Vector2Int GetTopLeftForCenteredPlacement(Vector2Int hoveredTile, InventoryLoot item)
    {
        GetRotatedSize(item, out int w, out int h);

        int offsetX = w / 2;
        int offsetY = h / 2;

        return new Vector2Int(hoveredTile.x - offsetX, hoveredTile.y - offsetY);
    }

    // ---- Placement Preview API ----

    public void ClearPlacementPreview()
    {
        for (int i = 0; i < placementUsed; i++)
            placementPool[i].gameObject.SetActive(false);

        placementUsed = 0;
    }

    public void ShowPlacementPreview(InventoryLoot item, int topLeftX, int topLeftY)
    {
        // Only clears placement (not rarity)
        for (int i = 0; i < placementUsed; i++)
            placementPool[i].gameObject.SetActive(false);
        placementUsed = 0;

        if (item == null || placementHighlightRoot == null || placementTilePrefab == null)
            return;

        Color col;
        if (Fits(item, topLeftX, topLeftY))
            col = validColor;
        else if (TryGetSingleOverlappedItem(item, topLeftX, topLeftY, out InventoryLoot overlapped) &&
                 CanPlaceIgnoring(item, topLeftX, topLeftY, overlapped))
            col = swapColor;
        else
            col = invalidColor;

        foreach (var cell in GetFootprintCells(item))
        {
            int gx = topLeftX + cell.x;
            int gy = topLeftY + cell.y;
            if (!IsInBounds(gx, gy)) continue;

            Image img;
            if (placementUsed >= placementPool.Count)
            {
                img = Instantiate(placementTilePrefab, placementHighlightRoot);
                img.raycastTarget = false;
                placementPool.Add(img);
            }
            else
            {
                img = placementPool[placementUsed];
                img.transform.SetParent(placementHighlightRoot, false);
            }
            placementUsed++;

            img.color = col;

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            rt.anchoredPosition = new Vector2(gx * 64f, -(gy * 64f));
            rt.sizeDelta = new Vector2(64f, 64f);

            img.gameObject.SetActive(true);
        }
    }

    public bool TryFindItemTopLeftAt(Vector2Int anyCell, out InventoryLoot item, out Vector2Int topLeft)
    {
        item = null;
        topLeft = default;

        if (!IsInBounds(anyCell.x, anyCell.y))
            return false;

        item = inventoryLootSlot[anyCell.x, anyCell.y];
        if (item == null)
            return false;

        // Find top-left by scanning all cells occupied by this item
        int minX = int.MaxValue;
        int minY = int.MaxValue;

        for (int x = 0; x < inventoryLootSlot.GetLength(0); x++)
            for (int y = 0; y < inventoryLootSlot.GetLength(1); y++)
            {
                if (inventoryLootSlot[x, y] != item)
                    continue;

                if (x < minX) minX = x;
                if (y < minY) minY = y;
            }

        if (minX == int.MaxValue)
            return false;

        topLeft = new Vector2Int(minX, minY);
        return true;
    }
    public bool TryFindFirstSpot(InventoryLoot item, out Vector2Int topLeft)
    {
        topLeft = default;
        if (item == null) return false;

        int width = inventoryLootSlot.GetLength(0);
        int height = inventoryLootSlot.GetLength(1);

        for (int y = 0; y < height; y++)        // top to bottom
        {
            for (int x = 0; x < width; x++)     // left to right
            {
                if (Fits(item, x, y))
                {
                    topLeft = new Vector2Int(x, y);
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryAddItem(Item data)
    {
        if (data == null) return false;
        if (inventoryLootPrefab == null) return false;

        InventoryLoot loot = Instantiate(inventoryLootPrefab, rectTransform);
        loot.Apply(data);

        if (TryFindFirstSpot(loot, out Vector2Int spot) && TryPlaceItem(loot, spot.x, spot.y))
            return true;

        Destroy(loot.gameObject);
        return false;
    }
    
    // ---- Random spawn ----

    public bool TrySpawnRandomLootItem()
    {
        if (inventoryLootPrefab == null || possibleItems == null || possibleItems.Length == 0)
            return false;

        Item data = null;
        for (int tries = 0; tries < 50; tries++)
        {
            Item candidate = possibleItems[Random.Range(0, possibleItems.Length)];
            if (candidate != null && candidate.type == ItemType.Loot)
            {
                data = candidate;
                break;
            }
        }

        if (data == null)
            return false;

        InventoryLoot loot = Instantiate(inventoryLootPrefab, rectTransform);
        loot.Apply(data);

        if (TryFindFirstSpot(loot, out Vector2Int spot) && TryPlaceItem(loot, spot.x, spot.y))
            return true;

        Destroy(loot.gameObject);
        return false;
    }

    private Color GetRarityColor(InventoryLoot loot)
    {
        if (loot == null || loot.item == null)
            return commonTileColor;

        return loot.item.rarity switch
        {
            ItemRarity.Junk => commonTileColor,
            ItemRarity.Valuable => uncommonTileColor,
            ItemRarity.Unusual => rareTileColor,
            ItemRarity.Exotic => legendaryTileColor,
            ItemRarity.Cursed => cursedTileColor,
            _ => commonTileColor
        };
    }

    public void ShowRarityTiles(InventoryLoot ignore = null)
    {
        // Only clears rarity (not placement)
        for (int i = 0; i < rarityUsed; i++)
            rarityPool[i].gameObject.SetActive(false);
        rarityUsed = 0;

        int width = inventoryLootSlot.GetLength(0);
        int height = inventoryLootSlot.GetLength(1);

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                InventoryLoot loot = inventoryLootSlot[x, y];
                if (loot == null || loot == ignore)
                    continue;

                Image img;
                if (rarityUsed >= rarityPool.Count)
                {
                    img = Instantiate(rarityTilePrefab, rarityHighlightRoot);
                    img.raycastTarget = false;
                    rarityPool.Add(img);
                }
                else
                {
                    img = rarityPool[rarityUsed];
                    img.transform.SetParent(rarityHighlightRoot, false);
                }
                rarityUsed++;

                img.color = GetRarityColor(loot);

                RectTransform rt = img.rectTransform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);

                rt.anchoredPosition = new Vector2(x * 64f, -(y * 64f));
                rt.sizeDelta = new Vector2(64f, 64f);

                img.gameObject.SetActive(true);
            }

        // ---- Draw hover highlight OFFICIALLY here, after rarity loop! ----
        if (hoverX >= 0 && hoverY >= 0 && IsInBounds(hoverX, hoverY) && inventoryLootSlot[hoverX, hoverY] != null)
        {
            // Use placement overlay pool for white hover highlight
            // (Don't clear the pool here; let the controller clear placementPool as before ShowPlacementPreview)
            Image img;
            if (placementUsed >= placementPool.Count)
            {
                img = Instantiate(placementTilePrefab, placementHighlightRoot);
                img.raycastTarget = false;
                placementPool.Add(img);
            }
            else
            {
                img = placementPool[placementUsed];
                img.transform.SetParent(placementHighlightRoot, false);
            }
            placementUsed++;

            img.color = new Color(1f, 1f, 1f, 0.4f); // white highlight

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            rt.anchoredPosition = new Vector2(hoverX * 64f, -(hoverY * 64f));
            rt.sizeDelta = new Vector2(64f, 64f);

            img.gameObject.SetActive(true);
        }
    }
    public void ShowHoverHighlight()
    {
        // Only one highlight, so clear only this one!
        for (int i = 0; i < placementUsed; i++)
            placementPool[i].gameObject.SetActive(false);
        placementUsed = 0;

        if (hoverX >= 0 && hoverY >= 0 && IsInBounds(hoverX, hoverY) && inventoryLootSlot[hoverX, hoverY] != null)
        {
            Image img;
            if (placementUsed >= placementPool.Count)
            {
                img = Instantiate(placementTilePrefab, placementHighlightRoot);
                img.raycastTarget = false;
                placementPool.Add(img);
            }
            else
            {
                img = placementPool[placementUsed];
                img.transform.SetParent(placementHighlightRoot, false);
            }
            placementUsed++;

            img.color = new Color(1f, 1f, 1f, 0.4f); // white

            RectTransform rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(hoverX * 64f, -(hoverY * 64f));
            rt.sizeDelta = new Vector2(64f, 64f);

            img.gameObject.SetActive(true);
        }
    }
}
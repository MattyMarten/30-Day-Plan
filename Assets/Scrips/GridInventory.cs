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
    [SerializeField] private RectTransform highlightRoot;   // child under grid
    [SerializeField] private Image highlightTilePrefab;     // 64x64 image prefab
    [SerializeField] private Color validColor = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.25f);

    private readonly List<Image> highlightPool = new();
    private int highlightUsed;

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
        {
            if (c.x < 0 || c.y < 0 || c.x >= baseW || c.y >= baseH)
                continue;

            yield return RotateCell(c, baseW, baseH, loot.rotation);
        }
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
        for (int i = 0; i < highlightUsed; i++)
            highlightPool[i].gameObject.SetActive(false);

        highlightUsed = 0;
    }

    public void ShowPlacementPreview(InventoryLoot item, int topLeftX, int topLeftY)
    {
        ClearPlacementPreview();

        if (item == null)
            return;

        if (highlightRoot == null || highlightTilePrefab == null)
            return;

        bool fits = Fits(item, topLeftX, topLeftY);
        Color col = fits ? validColor : invalidColor;

        foreach (var cell in GetFootprintCells(item))
        {
            int gx = topLeftX + cell.x;
            int gy = topLeftY + cell.y;

            if (!IsInBounds(gx, gy))
                continue;

            Image img = GetHighlightTile();
            img.color = col;

            RectTransform rt = img.rectTransform;
            rt.SetParent(highlightRoot, false);

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);

            rt.anchoredPosition = new Vector2(gx * tileSizeWidth, -(gy * tileSizeHeight));
            rt.sizeDelta = new Vector2(tileSizeWidth, tileSizeHeight);

            img.gameObject.SetActive(true);
        }
    }

    private Image GetHighlightTile()
    {
        if (highlightUsed >= highlightPool.Count)
        {
            Image created = Instantiate(highlightTilePrefab);
            created.raycastTarget = false;
            highlightPool.Add(created);
        }

        return highlightPool[highlightUsed++];
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
}
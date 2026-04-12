using UnityEngine;
using System.Collections.Generic;


public class PlayerInventory : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 6;

    [System.NonSerialized]
    public InventoryLoot[,] gridItems;

    private void Awake()
    {
        gridItems = new InventoryLoot[gridWidth, gridHeight];
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < gridWidth && y < gridHeight;
    }

    // Add item (1x1 cell version; expand for shapes later!)
    public bool AddItem(InventoryLoot item, int x, int y)
    {
        if (IsInBounds(x, y) && gridItems[x, y] == null)
        {
            gridItems[x, y] = item;
            return true;
        }
        return false;
    }

    // Remove the item at (x, y)
    public InventoryLoot RemoveItem(int x, int y)
    {
        if (IsInBounds(x, y))
        {
            InventoryLoot removed = gridItems[x, y];
            gridItems[x, y] = null;
            return removed;
        }
        return null;
    }

    // Remove an entire multi-cell item (all positions it occupies)
    public void RemoveMultiCellItem(InventoryLoot item)
    {
        for (int x = 0; x < gridWidth; x++)
        for (int y = 0; y < gridHeight; y++)
        {
            if (gridItems[x, y] == item)
                gridItems[x, y] = null;
        }
    }

    public InventoryLoot GetItemAt(int x, int y)
    {
        return IsInBounds(x, y) ? gridItems[x, y] : null;
    }
}

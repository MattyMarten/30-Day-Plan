using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.NonSerialized]
    public InventoryLoot[,] gridItems;

    public bool IsInBounds(int x, int y)
    {
        return gridItems != null 
            && x >= 0 && y >= 0 
            && x < gridItems.GetLength(0) && y < gridItems.GetLength(1);
    }

    public bool AddItem(InventoryLoot item, int x, int y)
    {
        if (IsInBounds(x, y) && gridItems[x, y] == null)
        {
            gridItems[x, y] = item;
            return true;
        }
        return false;
    }

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

    public void RemoveMultiCellItem(InventoryLoot item)
    {
        if (gridItems == null) return;
        for (int x = 0; x < gridItems.GetLength(0); x++)
        for (int y = 0; y < gridItems.GetLength(1); y++)
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
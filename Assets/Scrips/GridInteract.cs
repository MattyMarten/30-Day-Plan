using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GridInventory))]
public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GridInventoryControls controls;
    private GridInventory grid;

    private void Awake()
    {
        controls = Object.FindAnyObjectByType<GridInventoryControls>();
        grid = GetComponent<GridInventory>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (controls != null)
            controls.SetSelectedGrid(grid);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (controls != null)
            controls.SetSelectedGrid(null);
    }
}
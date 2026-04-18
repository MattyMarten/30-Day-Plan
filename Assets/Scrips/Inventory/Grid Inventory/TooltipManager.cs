using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TooltipManager : MonoBehaviour
{
    public TooltipPanel tooltipPanel; // Your tooltip script reference
    public GameObject gridRoot; // The root GameObject of your inventory grid or panel

    void Update()
    {
        // Only check if tooltip is currently shown
        if (!tooltipPanel.gameObject.activeSelf) return;

        // Is pointer over any UI?
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            tooltipPanel.Hide();
            return;
        }

        // Raycast all objects under the pointer
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        bool overGrid = false;
        foreach (var result in results)
        {
            // Check all parents in hierarchy in case items are children of grid
            Transform t = result.gameObject.transform;
            while (t != null)
            {
                if (t.gameObject == gridRoot)
                {
                    overGrid = true;
                    break;
                }
                t = t.parent;
            }
            if (overGrid) break;
        }

        // If not over grid or its children, hide tooltip
        if (!overGrid)
        {
            tooltipPanel.Hide();
        }
    }
}

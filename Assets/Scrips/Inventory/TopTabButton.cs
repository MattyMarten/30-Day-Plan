using UnityEngine;
using UnityEngine.EventSystems;

public class TopTabButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private InventoryControls controls;
    [SerializeField] private int pageIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controls == null)
        {
            Debug.LogWarning($"{nameof(TopTabButton)}: controls not assigned.", this);
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        controls.OpenAndGoToPage(pageIndex);
    }
}
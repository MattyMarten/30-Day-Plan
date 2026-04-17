using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipGridExitHandler : MonoBehaviour, IPointerExitHandler

{
    public TooltipPanel tooltipPanel;

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.Hide();
    }
}


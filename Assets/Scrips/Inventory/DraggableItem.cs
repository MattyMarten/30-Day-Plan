using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;
    [HideInInspector] public Item item;
    [HideInInspector] public Transform parentAfterDrag;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin Drag");

        parentAfterDrag = transform.parent;
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        if (image != null)
            image.raycastTarget = false;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Dragging");
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End Drag");

        transform.SetParent(parentAfterDrag, true);
        rectTransform.anchoredPosition = Vector2.zero;

        if (image != null)
            image.raycastTarget = true;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        InventoryManager inventoryManager = FindAnyObjectByType<InventoryManager>();
        if (inventoryManager != null)
    {
        inventoryManager.RefreshHeldItem();
    }
    }
}
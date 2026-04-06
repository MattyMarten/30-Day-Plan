using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;

    [HideInInspector] public Item item;
    [HideInInspector] public Transform parentAfterDrag;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [HideInInspector] public InventoryManager inventoryManager;

    public void InitialiseItem(Item newItem)
    {
        item = newItem;

        if (image != null && newItem != null)
            image.sprite = newItem.image;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (image == null)
            image = GetComponent<Image>();

        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null)
            return;

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
        if (canvas == null || rectTransform == null)
            return;

        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parentAfterDrag != null)
            transform.SetParent(parentAfterDrag, true);

        if (rectTransform != null)
            rectTransform.anchoredPosition = Vector2.zero;

        if (image != null)
            image.raycastTarget = true;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // Refresh held item after drop
        if (inventoryManager != null)
        {
            inventoryManager.RefreshHeldItem();
        }
        else
        {
            Debug.LogWarning($"{nameof(DraggableItem)}: inventoryManager was not set. (Did this item get spawned by InventoryManager?)", this);
        }
    }
}
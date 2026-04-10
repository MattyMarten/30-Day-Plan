using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image image;

    [HideInInspector] public Item item;
    [HideInInspector] public Transform parentAfterDrag;
    [SerializeField] private RectTransform dragParent;

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
    public void SetDragParent(RectTransform parent) => dragParent = parent;

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
        parentAfterDrag = transform.parent;

        if (dragParent != null)
            transform.SetParent(dragParent, true);

        transform.SetAsLastSibling();

        if (image != null) image.raycastTarget = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        RectTransform space = dragParent != null ? dragParent : (rectTransform.root as RectTransform);
        if (space == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                space,
                eventData.position,
                null, // overlay
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
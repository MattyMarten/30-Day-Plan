// using UnityEngine.UI;
// using UnityEngine;
// using UnityEngine.EventSystems;

// public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
// {
//     public Item item;

//     [Header("UI")]
//     public Image image;

//     [HideInInspector] public Transform parentAfterDrag;

//     private void Start()
//     {
//         InitialiseItem(item);
//     }

//     public void InitialiseItem(Item NewItem)
//     {
//         image.sprite = NewItem.image;
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         image.raycastTarget = false;
//         parentAfterDrag = transform.parent;
//         transform.SetParent(transform.root);
//     }

//     public void OnDrag(PointerEventData eventData)
//     {
//         transform.position = Input.mousePosition;
//     }

//     public void OnEndDrag(PointerEventData eventData)
//     {
//         image.raycastTarget = true;
//         transform.SetParent(parentAfterDrag);
//     }

// }

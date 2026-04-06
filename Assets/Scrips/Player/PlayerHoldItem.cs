using UnityEngine;

public class PlayerHoldItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform handPoint;

    private GameObject currentHeldItem;

    public void HoldItem(GameObject heldPrefab)
    {
        ClearHeldItem();

        if (heldPrefab == null)
            return;

        if (handPoint == null)
        {
            Debug.LogWarning($"{nameof(PlayerHoldItem)}: handPoint is not assigned.", this);
            return;
        }

        currentHeldItem = Instantiate(heldPrefab, handPoint);
        currentHeldItem.transform.localPosition = Vector3.zero;
        currentHeldItem.transform.localRotation = Quaternion.identity;
        currentHeldItem.transform.localScale = Vector3.one;
    }

    public void ClearHeldItem()
    {
        if (currentHeldItem == null)
            return;

        Destroy(currentHeldItem);
        currentHeldItem = null;
    }
}
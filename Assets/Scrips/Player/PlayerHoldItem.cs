using UnityEngine;

public class PlayerHoldItem : MonoBehaviour
{
    public Transform handPoint;
    private GameObject currentHeldItem;

    public void HoldItem(GameObject heldPrefab)
    {
        ClearHeldItem();

        if (heldPrefab != null)
        {
            currentHeldItem = Instantiate(heldPrefab, handPoint);
            currentHeldItem.transform.localPosition = Vector3.zero;
            currentHeldItem.transform.localRotation = Quaternion.identity;
            currentHeldItem.transform.localScale = Vector3.one;
        }
    }
    public void ClearHeldItem()
    {
        if (currentHeldItem != null)
        {
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }
    }
}

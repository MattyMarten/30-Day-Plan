using UnityEngine;
using StarterAssets;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public StarterAssetsInputs input;
    public Camera playerCamera;
    public GameObject interactText;

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayer))
        {
            ItemPickup pickup = hit.collider.GetComponent<ItemPickup>();

            if (pickup != null)
            {
                interactText.SetActive(true);

                if (input.interact)
                {
                    pickup.PickUp();
                    input.interact = false;
                }
                return;
            }
        }
        interactText.SetActive(false);
    }
}
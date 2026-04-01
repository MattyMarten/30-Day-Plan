using UnityEngine;
using StarterAssets;

public class InventoryControls : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject inventoryPage;
    private StarterAssetsInputs input;
    private FirstPersonController firstPersonController;
    public GameObject[] pages;
    private int currentPage = 0;
    private bool isOpen;

    private void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        firstPersonController = GetComponent<FirstPersonController>();

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);   
        }
        ShowPage(0);
        isOpen = false;
    }

    private void Update()
    {
        if (input != null && input.inventory)
        {
            ToggleInventory();
            input.inventory = false;
        }

        if (!isOpen || input == null)
            return;

        if (input.RightPage)
        {
            RightPage();
            input.RightPage = false;
        }

        if (input.LeftPage)
        {
            LeftPage();
            input.LeftPage = false;
        }
    }
    private void ToggleInventory()
    {
        isOpen = !isOpen;

        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isOpen);
        }

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (firstPersonController != null)
        {
            firstPersonController.enabled = !isOpen;
        }
    }
    private void ShowPage(int pageIndex)
    {
        if (pages == null || pages.Length == 0)
            return;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == pageIndex);
            }
        }

        currentPage = pageIndex;
    }

    private void LeftPage()
    {
        int newPage = currentPage + 1;

        if (newPage >= pages.Length)
            newPage = 0;

        ShowPage(newPage);
    }

    private void RightPage()
    {
        int newPage = currentPage - 1;

        if (newPage < 0)
            newPage = pages.Length - 1;

        ShowPage(newPage);
    }
}

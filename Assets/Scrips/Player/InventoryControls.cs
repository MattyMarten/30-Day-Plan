using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class InventoryControls : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inventoryUI;

    [Header("Pages (content panels). Same order as tabs.")]
    [SerializeField] private GameObject[] pages;

    [Header("Top Tabs (the Image objects with Button+Text). Same order as pages.")]
    [SerializeField] private Image[] tabImages;
    [SerializeField] private Color tabSelectedColor = Color.red;
    [SerializeField] private Color tabNormalColor = Color.white;

    [Header("Optional References (auto-found on same GameObject if missing)")]
    [SerializeField] private StarterAssetsInputs input;

    [Header("Optional: Grid controls (same GameObject or assigned)")]
    [SerializeField] private GridInventoryControls gridControls;

    private int currentPage = 0;
    private bool isOpen;

    private void Awake()
    {
        if (input == null)
            input = GetComponent<StarterAssetsInputs>();

        if (gridControls == null)
            gridControls = GetComponent<GridInventoryControls>();

        SetOpen(false);
        ShowPage(0);
    }

    private void Update()
    {
        if (input == null)
            return;

        if (input.ConsumeInventory())
            SetOpen(!isOpen);

        if (!isOpen)
            return;

        // Block gameplay input while menu is open
        input.jump = false;
        input.sprint = false;
        input.crouch = false;
        input.move = Vector2.zero;
        input.look = Vector2.zero;

        // Q / E page cycling
        if (input.ConsumeRightPage())
            NextPage();

        if (input.ConsumeLeftPage())
            PreviousPage();
    }

    // Call this from your tab Buttons
    public void OpenAndGoToPage(int pageIndex)
    {
        if (!isOpen)
            SetOpen(true);

        ShowPage(pageIndex);
    }

    private void SetOpen(bool open)
    {
        isOpen = open;

        if (inventoryUI != null)
            inventoryUI.SetActive(isOpen);

        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;

        if (input != null)
            input.uiBlocked = isOpen;

        // Let grid controls know whether it should run
        if (gridControls != null)
            gridControls.SetUIOpen(isOpen);

        if (isOpen && input != null)
        {
            input.jump = false;
            input.sprint = false;
            input.crouch = false;
            input.move = Vector2.zero;
            input.look = Vector2.zero;
        }
    }

    private void ShowPage(int pageIndex)
    {
        if (pages == null || pages.Length == 0)
            return;

        if (pageIndex < 0) pageIndex = pages.Length - 1;
        else if (pageIndex >= pages.Length) pageIndex = 0;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
                pages[i].SetActive(i == pageIndex);
        }

        currentPage = pageIndex;
        RefreshTabColors();
    }

    private void RefreshTabColors()
    {
        if (tabImages == null || tabImages.Length == 0)
            return;

        for (int i = 0; i < tabImages.Length; i++)
        {
            if (tabImages[i] != null)
                tabImages[i].color = (i == currentPage) ? tabSelectedColor : tabNormalColor;
        }
    }

    private void NextPage() => ShowPage(currentPage + 1);
    private void PreviousPage() => ShowPage(currentPage - 1);
}
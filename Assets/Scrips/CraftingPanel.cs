using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingPanel : MonoBehaviour
{
    public Slider amountSlider;
    public TMP_Text amountText;

    void Start()
    {
        // Register callback for slider value change
        amountSlider.onValueChanged.AddListener(OnAmountSliderChanged);

        // Set initial value text
        OnAmountSliderChanged(amountSlider.value);
    }

    void OnAmountSliderChanged(float newValue)
    {
        int intValue = Mathf.RoundToInt(newValue);
        amountText.text = intValue.ToString();
    }
}
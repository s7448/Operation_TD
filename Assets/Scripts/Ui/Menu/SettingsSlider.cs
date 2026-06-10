using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingSlider : MonoBehaviour
{
    [Header("Components")]
    public Slider slider;
    public TextMeshProUGUI valueText;

    private void Start()
    {
        UpdateValueText(slider.value);
        slider.onValueChanged.AddListener(UpdateValueText);
    }

    private void UpdateValueText(float newValue)
    {
        valueText.text = newValue.ToString("0");
    }
}
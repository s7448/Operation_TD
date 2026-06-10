using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Image backgroundImage;
    public Image iconImage;
    public TextMeshProUGUI labelText;
    public Button button;

    [Header("Colors")]
    public Color normalBgColor = Color.white;
    public Color normalContentColor = Color.black;
    public Color hoverBgColor = Color.black;
    public Color hoverContentColor = Color.white;
    public Color disabledBgColor = new Color(0.95f, 0.96f, 0.96f);
    public Color disabledContentColor = new Color(0.61f, 0.64f, 0.69f);

    public void OnPointerEnter(PointerEventData e)
    {
        if (button != null && !button.interactable) return;
        if (backgroundImage) backgroundImage.color = hoverBgColor;
        if (iconImage) iconImage.color = hoverContentColor;
        if (labelText) labelText.color = hoverContentColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (button != null && !button.interactable) return;
        if (backgroundImage) backgroundImage.color = normalBgColor;
        if (iconImage) iconImage.color = normalContentColor;
        if (labelText) labelText.color = normalContentColor;
    }

    public void SetInteractable(bool interactable)
    {
        if (button) button.interactable = interactable;
        if (backgroundImage) backgroundImage.color = interactable ? normalBgColor : disabledBgColor;
        if (iconImage) iconImage.color = interactable ? normalContentColor : disabledContentColor;
        if (labelText) labelText.color = interactable ? normalContentColor : disabledContentColor;
    }
}
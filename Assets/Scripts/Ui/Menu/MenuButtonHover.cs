using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image fillImage;       
    public Image iconImage;       
    public TextMeshProUGUI label; 

    [Header("Settings")]
    public float fadeDuration = 0.2f;

    private Coroutine colorCoroutine; 

    public void OnPointerEnter(PointerEventData eventData)
    {
        Button btn = GetComponent<Button>();
        if (btn != null && !btn.interactable) return;

        if (colorCoroutine != null) StopCoroutine(colorCoroutine);

        colorCoroutine = StartCoroutine(FadeColors(Color.black, Color.white));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);

        colorCoroutine = StartCoroutine(FadeColors(Color.white, Color.black));
    }

    private IEnumerator FadeColors(Color targetBgColor, Color targetFgColor)
    {
        Color currentBg = fillImage.color; 
        Color currentFg = iconImage.color; 
        float elapsedTime = 0f; 

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / fadeDuration; 

            fillImage.color = Color.Lerp(currentBg, targetBgColor, t);
            iconImage.color = Color.Lerp(currentFg, targetFgColor, t);
            label.color = Color.Lerp(currentFg, targetFgColor, t);

            yield return null; 
        }
        fillImage.color = targetBgColor;
        iconImage.color = targetFgColor;
        label.color = targetFgColor;
    }
}
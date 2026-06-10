using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyDetailsPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hardness UI Elements")]
    public CanvasGroup hardnessCanvasGroup;
    public Image hardnessFillBar; 

    public TextMeshProUGUI percentageText;

    [Header("Testing Variables")]
    public float targetFillAmount = 0.7f;

    private Coroutine animationCoroutine;

    private void Start()
    {
        hardnessCanvasGroup.alpha = 0f;
        hardnessFillBar.fillAmount = 0f;

        if (percentageText != null)
        {
            percentageText.text = "0%";
        }

        if (hardnessCanvasGroup != null)
        {
            hardnessCanvasGroup.gameObject.SetActive(false);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hardnessCanvasGroup != null)
        {
            hardnessCanvasGroup.gameObject.SetActive(true);
        }

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateHardness(1f, targetFillAmount, false));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateHardness(0f, 0f, true));
    }

    private IEnumerator AnimateHardness(float targetAlpha, float targetFill, bool turnOffWhenDone)
    {
        float startAlpha = hardnessCanvasGroup.alpha;
        float startFill = hardnessFillBar.fillAmount;
        float timeElapsed = 0f;
        float duration = 0.3f;

        int lastPercentage = -1;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float t = timeElapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            hardnessCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            float currentFill = Mathf.Lerp(startFill, targetFill, t);
            hardnessFillBar.fillAmount = currentFill;

            if (percentageText != null)
            {
                int currentPercentage = Mathf.RoundToInt(currentFill * 100f);

                if (currentPercentage != lastPercentage)
                {
                    percentageText.text = currentPercentage + "%";
                    lastPercentage = currentPercentage;
                }
            }

            yield return null;
        }

        hardnessCanvasGroup.alpha = targetAlpha;
        hardnessFillBar.fillAmount = targetFill;

        if (percentageText != null)
        {
            int finalPercentage = Mathf.RoundToInt(targetFill * 100f);
            percentageText.text = finalPercentage + "%";
        }

        if (turnOffWhenDone && hardnessCanvasGroup != null)
        {
            hardnessCanvasGroup.gameObject.SetActive(false);
        }

    }
}

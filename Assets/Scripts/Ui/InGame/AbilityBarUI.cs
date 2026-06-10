using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityBarUI : MonoBehaviour
{
    [System.Serializable]
    public class AbilityButtonUI
    {
        public AbilityType abilityType;
        public Button button;
        public Image iconImage;
        public Image cooldownOverlay;
        public TextMeshProUGUI cooldownText;
        public Image buttonBackground;
    }

    [Header("Ability Buttons")]
    [SerializeField] private AbilityButtonUI[] abilityButtons;

    private CanvasGroup canvasGroup;

    private static readonly Color ColorReady = Color.white;
    private static readonly Color ColorNotReady = new Color(0.5f, 0.5f, 0.5f);

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        HideBar();

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged += Refresh;
        else
            Debug.LogError("AbilityBarUI: AbilityManager not found in Awake!");

        foreach (var btn in abilityButtons)
        {
            AbilityType type = btn.abilityType;
            btn.button.onClick.AddListener(() => OnAbilityClicked(type));
        }
    }

    void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged -= Refresh;
    }

    private void HideBar()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowBar()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Refresh()
    {
        if (AbilityManager.Instance == null) return;

        bool anyUnlocked = false;

        foreach (var btn in abilityButtons)
        {
            bool isUnlocked = AbilityManager.Instance.IsUnlocked(btn.abilityType);
            bool isReady = AbilityManager.Instance.IsReady(btn.abilityType);
            int cooldown = AbilityManager.Instance.GetCooldownRemaining(btn.abilityType);
            int maxCooldown = AbilityManager.Instance.GetEffectiveCooldown(btn.abilityType);

            btn.button.gameObject.SetActive(isUnlocked);

            if (!isUnlocked) continue;

            anyUnlocked = true;
            btn.button.interactable = isReady;

            if (btn.buttonBackground != null)
                btn.buttonBackground.color = isReady ? ColorReady : ColorNotReady;

            if (btn.cooldownOverlay != null)
            {
                btn.cooldownOverlay.gameObject.SetActive(!isReady);
                if (!isReady && maxCooldown > 0)
                    btn.cooldownOverlay.fillAmount = (float)cooldown / maxCooldown;
                else
                    btn.cooldownOverlay.fillAmount = 0f;
            }

            if (btn.cooldownText != null)
            {
                btn.cooldownText.gameObject.SetActive(!isReady);
                btn.cooldownText.text = cooldown.ToString();
            }
        }

        if (anyUnlocked) ShowBar();
        else HideBar();
    }

    private void OnAbilityClicked(AbilityType type)
    {
        if (!AbilityManager.Instance.IsReady(type)) return;

        switch (type)
        {
            case AbilityType.Mine:
                AbilityPlacer.Instance.StartPlacingMine();
                break;
            case AbilityType.CzechHedgehog:
                AbilityPlacer.Instance.StartPlacingHedgehog();
                break;
            case AbilityType.Mortar:
                AbilityPlacer.Instance.StartMortarAim();
                break;
        }
    }
}
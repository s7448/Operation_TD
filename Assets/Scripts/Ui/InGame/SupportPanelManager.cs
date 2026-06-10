using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SupportPanelManager : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Button closeButton;

    [Header("Ability Cards")]
    [SerializeField] private AbilityCardUI mineCard;
    [SerializeField] private AbilityCardUI hedgehogCard;
    [SerializeField] private AbilityCardUI mortarCard;
    [SerializeField] private AbilityCardUI radarCard;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged += RefreshAll;
        else
            Debug.LogError("SupportPanelManager: AbilityManager not found!");

        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
    }

    void Start()
    {
        closeButton.onClick.AddListener(HidePanel);

        mineCard.Setup(AbilityType.Mine);
        hedgehogCard.Setup(AbilityType.CzechHedgehog);
        mortarCard.Setup(AbilityType.Mortar);
        radarCard.Setup(AbilityType.Radar);
    }
    void OnDestroy()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged -= RefreshAll;

        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
    }

    // -------------------------------------------------------
    // SHOW / HIDE

    public void ShowPanel()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        RefreshAll();
    }

    public void HidePanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsVisible => canvasGroup.alpha > 0f;

    // -------------------------------------------------------
    // REFRESH

    private void RefreshAll()
    {
        mineCard.Refresh();
        hedgehogCard.Refresh();
        mortarCard.Refresh();
        radarCard.Refresh();
    }

    private void OnMoneyChanged(int amount)
    {
        if (IsVisible) RefreshAll();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TowerBuildButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tower Setup")]
    [SerializeField] private TowerConfig towerConfig;
    [SerializeField] private GameObject towerPrefab;

    [Header("UI References")]
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image towerIconImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Colors")]
    public Color normalBgColor = Color.white;
    public Color normalContentColor = Color.black;
    public Color hoverBgColor = Color.black;
    public Color hoverContentColor = Color.white;
    public Color disabledBgColor = new Color(0.95f, 0.96f, 0.96f);
    public Color disabledContentColor = new Color(0.61f, 0.64f, 0.69f);

    // -------------------------------------------------------
    void Start()
    {
        SetupButton();

        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged += UpdateButtonState;

        UpdateButtonState();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.OnAbilityStateChanged -= UpdateButtonState;
    }

    // -------------------------------------------------------
    // SETUP

    private void SetupButton()
    {
        if (button == null) button = GetComponent<Button>();

        button.transition = Selectable.Transition.None;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);

        if (towerConfig == null)
        {
            Debug.LogError($"TowerBuildButton: No TowerConfig assigned on {gameObject.name}!");
            return;
        }

        if (nameText != null)
            nameText.text = towerConfig.towerName;

        if (costText != null)
            costText.text = $"${towerConfig.cost}";

        if (towerIconImage != null && towerConfig.icon != null)
            towerIconImage.sprite = towerConfig.icon;
    }

    // -------------------------------------------------------
    // HOVER

    public void OnPointerEnter(PointerEventData e)
    {
        if (!button.interactable) return;

        if (backgroundImage) backgroundImage.color = hoverBgColor;
        if (towerIconImage) towerIconImage.color = hoverContentColor;
        if (nameText) nameText.color = hoverContentColor;
        if (costText) costText.color = hoverContentColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!button.interactable) return;

        if (backgroundImage) backgroundImage.color = normalBgColor;
        if (towerIconImage) towerIconImage.color = normalContentColor;
        if (nameText) nameText.color = normalContentColor;
        if (costText) costText.color = normalContentColor;
    }

    // -------------------------------------------------------
    // STATE

    private void OnMoneyChanged(int newAmount)
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (button == null || towerConfig == null) return;

        bool canAfford = GameManager.Instance != null
            && GameManager.Instance.CanAfford(towerConfig.cost);

        bool radarCheck = true;
        if (towerPrefab != null && towerPrefab.GetComponent<RadarTower>() != null)
            radarCheck = AbilityManager.Instance != null
                         && AbilityManager.Instance.IsUnlocked(AbilityType.Radar)
                         && AbilityManager.Instance.CanPlaceRadar();

        bool canBuild = canAfford && radarCheck;

        button.interactable = canBuild;

        if (backgroundImage)
            backgroundImage.color = canBuild ? normalBgColor : disabledBgColor;
        if (towerIconImage)
            towerIconImage.color = canBuild ? normalContentColor : disabledContentColor;
        if (nameText)
            nameText.color = canBuild ? normalContentColor : disabledContentColor;
        if (costText)
            costText.color = canBuild ? normalContentColor : disabledContentColor;
    }

    // -------------------------------------------------------
    // CLICK

    private void OnButtonClicked()
    {
        if (towerConfig == null || towerPrefab == null) return;

        if (towerPrefab.GetComponent<RadarTower>() != null)
        {
            if (!AbilityManager.Instance.CanPlaceRadar())
            {
                Debug.Log("Radar not available — either not unlocked, on cooldown, or already placed!");
                return;
            }
        }

        if (BuildManager.Instance != null)
            BuildManager.Instance.StartPlacingTower(towerPrefab, towerConfig.cost);
    }
}
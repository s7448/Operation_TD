using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelManager : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Button closeButton;

    [Header("Targeting")]
    [SerializeField] private TextMeshProUGUI currentTargetingText;
    [SerializeField] private Button prevTargetingButton;
    [SerializeField] private Button nextTargetingButton;

    [Header("Upgrade Paths")]
    [SerializeField] private UpgradePathUI[] pathUIs;

    [Header("Footer")]
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellButtonText;

    [Header("UI Sections")]
    [SerializeField] private GameObject targetingSection;

    private CombatTower selectedTower;
    private TowerUpgradeController upgradeController;
    private Targeting targetingSystem;

    // -------------------------------------------------------
    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        HidePanel();
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.OnTowerSelected += OpenPanel;
            InteractionManager.Instance.OnTowerDeselected += ClosePanel;
            InteractionManager.Instance.OnRadarSelected += OpenRadarPanel;
            InteractionManager.Instance.OnRadarDeselected += ClosePanel;
        }
        else
        {
            Debug.LogError("InteractionManager NULL!");
        }
    }

    void Start()
    {
        Debug.Log($"UpgradePanelManager Start — InteractionManager: {InteractionManager.Instance}");
        Debug.Log($"UpgradePanelManager Start — closeButton: {closeButton}");
        Debug.Log($"UpgradePanelManager Start — sellButton: {sellButton}");
        closeButton.onClick.AddListener(ClosePanel);
        sellButton.onClick.AddListener(SellTower);
        prevTargetingButton.onClick.AddListener(CyclePrevTargeting);
        nextTargetingButton.onClick.AddListener(CycleNextTargeting);

        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.OnTowerSelected += OpenPanel;
            InteractionManager.Instance.OnTowerDeselected += ClosePanel;
            Debug.Log("UpgradePanelManager: Subscribed to InteractionManager");
        }
        else
        {
            Debug.LogError("UpgradePanelManager: InteractionManager.Instance is NULL in Start!");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
    }

    void OnDisable()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.OnTowerSelected -= OpenPanel;
            InteractionManager.Instance.OnTowerDeselected -= ClosePanel;
        }
        if (GameManager.Instance != null)
            GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
    }

    // -------------------------------------------------------
    // OPEN / CLOSE
    private void HidePanel()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void ShowPanel()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void OpenPanel(CombatTower tower)
    {
        Debug.Log($"OpenPanel fired! Tower: {tower?.name}");
        selectedTower = tower;
        upgradeController = tower.GetComponent<TowerUpgradeController>();
        targetingSystem = tower.GetComponent<Targeting>();

        if (upgradeController == null)
        {
            Debug.LogError($"{tower.name} is missing TowerUpgradeController!");
            return;
        }

        gameObject.SetActive(true);

        towerNameText.text = tower.config.towerName.ToUpper();

        UpdateTargetingUI();

        UpdateSellButton();

        for (int i = 0; i < pathUIs.Length; i++)
            pathUIs[i].Setup(i, upgradeController);

        ShowPanel();
    }

    public void OpenRadarPanel(RadarTower radar)
    {
        Debug.Log($"OpenRadarPanel called — radar: {radar?.name}");

        selectedTower = null;
        upgradeController = radar.GetComponent<TowerUpgradeController>();
        targetingSystem = null;

        if (targetingSection != null)
            targetingSection.SetActive(false);

        ShowPanel();

        towerNameText.text = "RADAR";
        UpdateSellButton();

        for (int i = 0; i < pathUIs.Length; i++)
            pathUIs[i].Setup(i, upgradeController);
    }

    private void ClosePanel()
    {
        selectedTower = null;
        upgradeController = null;
        targetingSystem = null;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // -------------------------------------------------------
    // TARGETING

    private void CycleNextTargeting()
    {
        if (targetingSystem == null) return;
        targetingSystem.CycleTargetingMode();
        UpdateTargetingUI();
    }

    private void CyclePrevTargeting()
    {
        if (targetingSystem == null) return;

        int current = (int)targetingSystem.targetingMode;
        int total = System.Enum.GetValues(typeof(TargetingMode)).Length;
        targetingSystem.targetingMode = (TargetingMode)((current - 1 + total) % total);

        UpdateTargetingUI();
    }

    private void UpdateTargetingUI()
    {
        if (targetingSystem == null || currentTargetingText == null) return;
        currentTargetingText.text = targetingSystem.targetingMode.ToString().ToUpper();
    }

    // -------------------------------------------------------
    // SELL

    private void UpdateSellButton()
    {
        if (selectedTower == null) return;

        int refund = Mathf.RoundToInt(selectedTower.config.cost * 0.5f);

        if (upgradeController != null && selectedTower.config.upgradeTree != null)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < upgradeController.currentLevels[i]; j++)
                    refund += Mathf.RoundToInt(
                        selectedTower.config.upgradeTree.paths[i].steps[j].cost * 0.5f);
        }

        sellButtonText.text = $"SELL TOWER (${refund})";
    }

    private void SellTower()
    {
        if (selectedTower == null) return;

        int refund = Mathf.RoundToInt(selectedTower.config.cost * 0.5f);

        if (upgradeController != null && selectedTower.config.upgradeTree != null)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < upgradeController.currentLevels[i]; j++)
                    refund += Mathf.RoundToInt(
                        selectedTower.config.upgradeTree.paths[i].steps[j].cost * 0.5f);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(refund);

        Destroy(selectedTower.gameObject);
        ClosePanel();
    }

    // -------------------------------------------------------
    // MONEY CHANGED

    void OnMoneyChanged(int newAmount)
    {
        if (!gameObject.activeSelf || upgradeController == null) return;
        foreach (var ui in pathUIs)
            ui.RefreshVisuals();
        UpdateSellButton();
    }
}
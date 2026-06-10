using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePathUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI pathNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI upgradeButtonText;
    [SerializeField] private Image[] levelBars;         
    [SerializeField] private Image upgradeBackground;   
    [SerializeField] private Button upgradeButton;

    private static readonly Color ColorActive = Color.black;
    private static readonly Color ColorInactive = new Color(0.898f, 0.906f, 0.922f);
    private static readonly Color ColorDisabledBg = new Color(0.95f, 0.96f, 0.96f);
    private static readonly Color ColorDisabledText = new Color(0.61f, 0.64f, 0.69f);

    private int pathIndex;
    private TowerUpgradeController activeController;

    void Start()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    public void Setup(int index, TowerUpgradeController controller)
    {
        pathIndex = index;
        activeController = controller;
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        if (activeController == null) return;

        int currentLevel = activeController.currentLevels[pathIndex];
        UpgradeStep nextUpgrade = activeController.GetNextUpgrade(pathIndex);
        bool canUpgrade = activeController.CanUpgrade(pathIndex);

        var config = activeController.GetConfig();

        int maxLevels = 5;
        if (config?.upgradeTree != null && pathIndex < config.upgradeTree.paths.Length)
            maxLevels = config.upgradeTree.paths[pathIndex].steps.Length;

        levelText.text = $"{currentLevel}/{maxLevels}";

        if (config?.upgradeTree != null && pathIndex < config.upgradeTree.paths.Length)
        {
            string pathName = config.upgradeTree.paths[pathIndex].pathName.ToUpper();
            float statValue = GetCurrentStatValue(config);
            pathNameText.text = $"{pathName} {FormatStatValue(config, statValue)}";
        }


        for (int i = 0; i < levelBars.Length; i++)
        {
            if (levelBars[i] == null) continue;
            if (i >= maxLevels)
            {
                levelBars[i].gameObject.SetActive(false);
                continue;
            }
            levelBars[i].gameObject.SetActive(true);
            levelBars[i].color = i < currentLevel ? ColorActive : ColorInactive;
        }

        if (string.IsNullOrEmpty(nextUpgrade.upgradeName))
        {
            descriptionText.text = "Maximum level reached";
            upgradeButtonText.text = "MAX LEVEL";
            upgradeButton.interactable = false;
            upgradeBackground.color = ColorDisabledBg;
            upgradeButtonText.color = ColorDisabledText;
            return;
        }

        descriptionText.text = nextUpgrade.description;
        upgradeButtonText.text = $"UPGRADE (${nextUpgrade.cost})";

        upgradeButton.interactable = canUpgrade;
        upgradeBackground.color = canUpgrade ? Color.white : ColorDisabledBg;
        upgradeButtonText.color = canUpgrade ? Color.black : ColorDisabledText;
    }

    private float GetCurrentStatValue(TowerConfig config)
    {
        if (config?.upgradeTree == null) return 0f;

        var path = config.upgradeTree.paths[pathIndex];
        var combatConfig = config as CombatTowerConfig;

        float baseValue = path.stat switch
        {
            UpgradeStat.SoftAttack => combatConfig != null ? combatConfig.softAttack : 0f,
            UpgradeStat.HardAttack => combatConfig != null ? combatConfig.hardAttack : 0f,
            UpgradeStat.Piercing => combatConfig != null ? combatConfig.piercing : 0f,
            UpgradeStat.Range => config.range,
            UpgradeStat.FireRate => config.fireRate,
            _ => 0f
        };

        int currentLevel = activeController.currentLevels[pathIndex];
        for (int i = 0; i < currentLevel && i < path.steps.Length; i++)
            baseValue += path.steps[i].bonusAmount;

        return baseValue;
    }

    private string FormatStatValue(TowerConfig config, float value)
    {
        if (config?.upgradeTree == null) return value.ToString("F1");

        UpgradeStat stat = config.upgradeTree.paths[pathIndex].stat;
        return stat == UpgradeStat.FireRate ? value.ToString("F2") : value.ToString("F1");
    }


    void OnUpgradeClicked()
    {
        if (activeController == null) return;
        activeController.PerformUpgrade(pathIndex);
        RefreshVisuals();
    }
}
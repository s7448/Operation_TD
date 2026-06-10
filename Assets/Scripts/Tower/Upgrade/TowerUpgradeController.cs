using UnityEngine;

public class TowerUpgradeController : MonoBehaviour
{
    public int[] currentLevels = new int[3] { 0, 0, 0 };

    private TowerConfig config;
    public TowerConfig GetConfig() => config;
    private CombatTower combatTower;

    private const int MAX_LEVELS = 5;
    private const int MAX_TIER_SECONDARY = 3;
    private const int Max_PATHS = 3;

    void Start()
    {
        combatTower = GetComponent<CombatTower>();
        if (combatTower != null) config = combatTower.config;

        if (config == null)
        {
            var support = GetComponent<SupportTower>();
            if (support) config = support.config;
        }

        if (config == null)
        {
            var radar = GetComponent<RadarTower>();
            if (radar) config = radar.config;
        }
    }

    public bool CanUpgrade(int pathIndex)
    {
        if (config == null || config.upgradeTree == null) return false;
        if (pathIndex < 0 || pathIndex >= Max_PATHS) return false;

        int nextLevel = currentLevels[pathIndex];
        if (nextLevel >= config.upgradeTree.paths[pathIndex].steps.Length) return false;
        if (nextLevel >= MAX_LEVELS) return false;

        int activePaths = 0;
        for (int i = 0; i < Max_PATHS; i++)
            if (currentLevels[i] > 0) activePaths++;

        if (currentLevels[pathIndex] == 0 && activePaths >= 2) return false;

        bool hasPrimaryPath = false;
        for (int i = 0; i < Max_PATHS; i++)
            if (i != pathIndex && currentLevels[i] > MAX_TIER_SECONDARY)
                hasPrimaryPath = true;

        if (hasPrimaryPath && nextLevel >= MAX_TIER_SECONDARY) return false;

        int cost = config.upgradeTree.paths[pathIndex].steps[nextLevel].cost;
        if (GameManager.Instance != null && !GameManager.Instance.CanAfford(cost)) return false;

        return true;
    }

    public void PerformUpgrade(int pathIndex)
    {
        if (!CanUpgrade(pathIndex)) return;

        int levelIndex = currentLevels[pathIndex];
        UpgradeStep step = config.upgradeTree.paths[pathIndex].steps[levelIndex];

        if (GameManager.Instance != null)
            GameManager.Instance.SpendMoney(step.cost);

        currentLevels[pathIndex]++;
        ApplyUpgrade(pathIndex, step);
    }

    private void ApplyUpgrade(int pathIndex, UpgradeStep step)
    {
        if (combatTower == null) return;

        UpgradeStat stat = config.upgradeTree.paths[pathIndex].stat;
        float bonus = step.bonusAmount;

        switch (stat)
        {
            case UpgradeStat.SoftAttack:
                combatTower.ModifyStats(bonus, 0f, 0f, 0f);
                break;
            case UpgradeStat.HardAttack:
                combatTower.ModifyStats(0f, 0f, 0f, 0f, bonus);
                break;
            case UpgradeStat.Piercing:
                combatTower.ModifyStats(0f, 0f, 0f, bonus);
                break;
            case UpgradeStat.Range:
                combatTower.ModifyStats(0f, 0f, bonus, 0f);
                break;
            case UpgradeStat.FireRate:
                combatTower.ModifyStats(0f, bonus, 0f, 0f);
                break;
        }
    }

    public UpgradeStep GetNextUpgrade(int pathIndex)
    {
        int lvl = currentLevels[pathIndex];
        if (config?.upgradeTree != null && lvl < config.upgradeTree.paths[pathIndex].steps.Length)
            return config.upgradeTree.paths[pathIndex].steps[lvl];

        return new UpgradeStep();
    }

    public string GetPathName(int pathIndex)
    {
        if (config?.upgradeTree != null)
            return config.upgradeTree.paths[pathIndex].pathName;
        return "";
    }

    public UpgradeStat GetPathStat(int pathIndex)
    {
        if (config?.upgradeTree != null)
            return config.upgradeTree.paths[pathIndex].stat;
        return UpgradeStat.SoftAttack;
    }

    public void PerformUpgradeWithoutCost(int pathIndex)
    {
        if (config == null || config.upgradeTree == null) return;
        if (pathIndex < 0 || pathIndex >= 3) return;
        if (currentLevels[pathIndex] >= config.upgradeTree.paths[pathIndex].steps.Length) return;

        int levelIndex = currentLevels[pathIndex];
        UpgradeStep step = config.upgradeTree.paths[pathIndex].steps[levelIndex];
        currentLevels[pathIndex]++;
        ApplyUpgrade(pathIndex, step);
    }
}
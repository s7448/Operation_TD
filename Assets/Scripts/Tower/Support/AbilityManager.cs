using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [Header("Ability Data")]
    [SerializeField] private AbilityData mineData;
    [SerializeField] private AbilityData hedgehogData;
    [SerializeField] private AbilityData mortarData;

    [Header("Radar")]
    [SerializeField] private RadarConfig radarConfig;
    [SerializeField] private int radarUnlockCost = 500;
    [SerializeField] private int radarCooldownRounds = 10;

    private Dictionary<AbilityType, bool> unlockedAbilities = new Dictionary<AbilityType, bool>();

    private Dictionary<AbilityType, int> upgradeLevels = new Dictionary<AbilityType, int>();

    private Dictionary<AbilityType, int> cooldownsRemaining = new Dictionary<AbilityType, int>();

    private List<GameObject> placedMines = new List<GameObject>();
    private int minesExplodedThisRound = 0;
    private GameObject placedRadar = null;

    public event System.Action OnAbilityStateChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            unlockedAbilities[type] = false;
            upgradeLevels[type] = 0;
            cooldownsRemaining[type] = 0;
        }
    }

    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveEnded += OnRoundEnded;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveEnded -= OnRoundEnded;
    }

    // -------------------------------------------------------
    // UNLOCK

    public bool CanUnlock(AbilityType type)
    {
        if (unlockedAbilities[type]) return false;

        if (type == AbilityType.Radar)
            return GameManager.Instance != null &&
                   GameManager.Instance.CanAfford(radarUnlockCost);

        return GameManager.Instance != null &&
               GameManager.Instance.CanAfford(GetData(type).unlockCost);
    }


    public void UnlockAbility(AbilityType type)
    {
        if (!CanUnlock(type)) return;

        int cost = type == AbilityType.Radar ? radarUnlockCost : GetData(type).unlockCost;
        GameManager.Instance.SpendMoney(cost);
        unlockedAbilities[type] = true;
        OnAbilityStateChanged?.Invoke();
    }

    public bool IsUnlocked(AbilityType type) => unlockedAbilities[type];

    // -------------------------------------------------------
    // UPGRADES

    public bool CanUpgrade(AbilityType type)
    {
        if (!IsUnlocked(type)) return false;
        if (type == AbilityType.Radar) return false;

        int level = upgradeLevels[type];
        AbilityData data = GetData(type);

        if (level >= data.upgradeTiers.Length) return false;

        int cost = data.upgradeTiers[level].cost;
        return GameManager.Instance != null && GameManager.Instance.CanAfford(cost);
    }

    public void UpgradeAbility(AbilityType type)
    {
        if (!CanUpgrade(type)) return;

        int level = upgradeLevels[type];
        int cost = GetData(type).upgradeTiers[level].cost;

        GameManager.Instance.SpendMoney(cost);
        upgradeLevels[type]++;
        OnAbilityStateChanged?.Invoke();
    }

    public int GetUpgradeLevel(AbilityType type) => upgradeLevels[type];

    // -------------------------------------------------------
    // COOLDOWN

    public bool IsReady(AbilityType type)
    {
        if (!IsUnlocked(type)) return false;
        return cooldownsRemaining[type] <= 0;
    }

    public int GetCooldownRemaining(AbilityType type) => cooldownsRemaining[type];

    public int GetEffectiveCooldown(AbilityType type)
    {
        if (type == AbilityType.Radar) return radarCooldownRounds;

        AbilityData data = GetData(type);
        int cooldown = data.roundCooldown;

        int level = upgradeLevels[type];
        for (int i = 0; i < level; i++)
            cooldown -= data.upgradeTiers[i].cooldownReduction;

        return Mathf.Max(1, cooldown);
    }

    private void SetOnCooldown(AbilityType type)
    {
        cooldownsRemaining[type] = GetEffectiveCooldown(type);
        OnAbilityStateChanged?.Invoke();
    }

    // -------------------------------------------------------
    // ROUND END

    private void OnRoundEnded()
    {
        foreach (AbilityType type in System.Enum.GetValues(typeof(AbilityType)))
        {
            if (cooldownsRemaining[type] > 0)
            {
                cooldownsRemaining[type]--;
            }
        }

        minesExplodedThisRound = 0;

        OnAbilityStateChanged?.Invoke();
    }

    // -------------------------------------------------------
    // MINE

    public int GetMaxMines()
    {
        int max = mineData.baseMaxPlacements;
        int level = upgradeLevels[AbilityType.Mine];
        for (int i = 0; i < level; i++)
            max += mineData.upgradeTiers[i].maxPlacementsBonus;
        return max;
    }

    public int GetPlacedMineCount() => placedMines.Count;

    public bool CanPlaceMine() =>
        IsUnlocked(AbilityType.Mine) && placedMines.Count < GetMaxMines();

    public void RegisterMinePlaced(GameObject mine)
    {
        placedMines.Add(mine);
        OnAbilityStateChanged?.Invoke();
    }

    public void RegisterMineExploded(GameObject mine)
    {
        placedMines.Remove(mine);
        minesExplodedThisRound++;
        OnAbilityStateChanged?.Invoke();
    }

    public void RegisterMineRemoved(GameObject mine)
    {
        placedMines.Remove(mine);
        OnAbilityStateChanged?.Invoke();
    }

    // -------------------------------------------------------
    // MORTAR

    public float GetMortarDamage()
    {
        float dmg = mortarData.baseDamage;
        int level = upgradeLevels[AbilityType.Mortar];
        for (int i = 0; i < level; i++)
            dmg += mortarData.upgradeTiers[i].damageBonus;
        return dmg;
    }

    public float GetMortarRadius()
    {
        float radius = mortarData.baseRadius;
        int level = upgradeLevels[AbilityType.Mortar];
        for (int i = 0; i < level; i++)
            radius += mortarData.upgradeTiers[i].radiusBonus;
        return radius;
    }

    public void UseMortar()
    {
        SetOnCooldown(AbilityType.Mortar);
    }

    // -------------------------------------------------------
    // HEDGEHOG

    public float GetHedgehogSlow()
    {
        float slow = hedgehogData.baseSlowAmount;
        int level = upgradeLevels[AbilityType.CzechHedgehog];
        for (int i = 0; i < level; i++)
            slow += hedgehogData.upgradeTiers[i].slowBonus;
        return Mathf.Min(slow, 0.9f);
    }

    public float GetHedgehogDuration()
    {
        float duration = hedgehogData.baseDuration;
        int level = upgradeLevels[AbilityType.CzechHedgehog];
        for (int i = 0; i < level; i++)
            duration += hedgehogData.upgradeTiers[i].durationBonus;
        return duration;
    }

    public void UseHedgehog()
    {
        SetOnCooldown(AbilityType.CzechHedgehog);
    }

    // -------------------------------------------------------
    // RADAR

    public bool CanPlaceRadar() =>
        IsUnlocked(AbilityType.Radar) &&
        IsReady(AbilityType.Radar) &&
        placedRadar == null;

    public void RegisterRadarPlaced(GameObject radar)
    {
        placedRadar = radar;
        SetOnCooldown(AbilityType.Radar);
    }

    public void RegisterRadarDestroyed()
    {
        placedRadar = null;
    }

    // -------------------------------------------------------
    // HELPERS

    public AbilityData GetData(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Mine: return mineData;
            case AbilityType.CzechHedgehog: return hedgehogData;
            case AbilityType.Mortar: return mortarData;
            case AbilityType.Radar: return null;
            default: return null;
        }
    }

    public void ForceUnlock(AbilityType type)
    {
        unlockedAbilities[type] = true;
        OnAbilityStateChanged?.Invoke();
    }

    public void ForceUpgradeLevel(AbilityType type, int level)
    {
        upgradeLevels[type] = level;
    }

    public void ForceCooldown(AbilityType type, int rounds)
    {
        cooldownsRemaining[type] = rounds;
    }
}
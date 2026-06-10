using UnityEngine;
using System.Collections.Generic;

public class RadarTower : MonoBehaviour
{
    [Header("Configuration")]
    public TowerConfig config;

    [Header("Radar Settings")]
    [SerializeField] public float buffRadius = 8f;
    [SerializeField] private LayerMask towerLayer;

    private float rangeBuffPercent = 0f;
    private float fireRateBuffPercent = 0f;
    private float damageBuffPercent = 0f;

    private List<CombatTower> buffedTowers = new List<CombatTower>();

    private TowerUpgradeController upgradeController;

    void Start()
    {
        upgradeController = GetComponent<TowerUpgradeController>();

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.RegisterRadarPlaced(gameObject);
        else
            Debug.LogError("RadarTower: AbilityManager not found!");
    }

    void OnDestroy()
    {
        RemoveAllBuffs();

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.RegisterRadarDestroyed();
    }

    void Update()
    {
        UpdateBuffs();
    }

    // -------------------------------------------------------
    // BUFF CALCULATION

    private void CalculateBuffValues()
    {
        if (upgradeController == null || config?.upgradeTree == null) return;

        rangeBuffPercent = 0f;
        fireRateBuffPercent = 0f;
        damageBuffPercent = 0f;

        for (int path = 0; path < 3; path++)
        {
            int level = upgradeController.currentLevels[path];

            for (int i = 0; i < level; i++)
            {
                UpgradeStep step = config.upgradeTree.paths[path].steps[i];

                switch (path)
                {
                    case 0: rangeBuffPercent += step.bonusAmount; break;
                    case 1: fireRateBuffPercent += step.bonusAmount; break;
                    case 2: damageBuffPercent += step.bonusAmount; break;
                }
            }
        }
    }

    private void UpdateBuffs()
    {
        CalculateBuffValues();

        Collider[] hits = Physics.OverlapSphere(transform.position, buffRadius, towerLayer);
        List<CombatTower> towersInRange = new List<CombatTower>();

        foreach (Collider hit in hits)
        {
            CombatTower tower = hit.GetComponentInParent<CombatTower>();
            if (tower != null && tower.gameObject != gameObject)
                towersInRange.Add(tower);
        }

        List<CombatTower> toRemove = new List<CombatTower>();
        foreach (CombatTower buffed in buffedTowers)
        {
            if (!towersInRange.Contains(buffed))
            {
                RemoveBuff(buffed);
                toRemove.Add(buffed);
            }
        }
        foreach (CombatTower t in toRemove)
            buffedTowers.Remove(t);

        foreach (CombatTower tower in towersInRange)
        {
            ApplyBuff(tower);
            if (!buffedTowers.Contains(tower))
                buffedTowers.Add(tower);
        }
    }

    private void ApplyBuff(CombatTower tower)
    {
        if (tower == null) return;
        tower.SetRadarBuff(rangeBuffPercent, fireRateBuffPercent, damageBuffPercent);
    }

    private void RemoveBuff(CombatTower tower)
    {
        if (tower == null) return;
        tower.SetRadarBuff(0f, 0f, 0f);
    }

    private void RemoveAllBuffs()
    {
        foreach (CombatTower tower in buffedTowers)
            RemoveBuff(tower);
        buffedTowers.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, buffRadius);
    }
}
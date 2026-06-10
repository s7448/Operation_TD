using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private List<TowerPrefabMapping> towerPrefabs;

    [System.Serializable]
    public class TowerPrefabMapping
    {
        public string towerName;
        public GameObject prefab;
    }

    void Start()
    {
        if (SceneTransitionData.LoadSave)
        {
            SceneTransitionData.LoadSave = false;
            LoadGame();
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }
    }

    private void LoadGame()
    {
        string sceneName = SceneTransitionData.SaveSceneName;
        GameSaveData data = SaveManager.Instance?.LoadGame(sceneName);

        if (data == null)
        {
            Debug.LogError("No save data found!");
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
            GameManager.Instance.SetMoney(data.currentMoney);
            GameManager.Instance.SetLives(data.currentLives);
            GameManager.Instance.SetEnemiesDefeated(data.enemiesDefeated);
            GameManager.Instance.SetTimeElapsed(data.timeElapsed);
        }

        RestoreAbilities(data.abilities);

        StartCoroutine(RestoreTowersNextFrame(data));
    }

    private void RestoreAbilities(AbilitySaveData data)
    {
        if (AbilityManager.Instance == null || data == null) return;

        if (data.mineUnlocked) AbilityManager.Instance.ForceUnlock(AbilityType.Mine);
        if (data.hedgehogUnlocked) AbilityManager.Instance.ForceUnlock(AbilityType.CzechHedgehog);
        if (data.mortarUnlocked) AbilityManager.Instance.ForceUnlock(AbilityType.Mortar);
        if (data.radarUnlocked) AbilityManager.Instance.ForceUnlock(AbilityType.Radar);

        AbilityManager.Instance.ForceUpgradeLevel(AbilityType.Mine, data.mineUpgradeLevel);
        AbilityManager.Instance.ForceUpgradeLevel(AbilityType.CzechHedgehog, data.hedgehogUpgradeLevel);
        AbilityManager.Instance.ForceUpgradeLevel(AbilityType.Mortar, data.mortarUpgradeLevel);

        AbilityManager.Instance.ForceCooldown(AbilityType.Mine, data.mineCooldown);
        AbilityManager.Instance.ForceCooldown(AbilityType.CzechHedgehog, data.hedgehogCooldown);
        AbilityManager.Instance.ForceCooldown(AbilityType.Mortar, data.mortarCooldown);
        AbilityManager.Instance.ForceCooldown(AbilityType.Radar, data.radarCooldown);
    }

    private IEnumerator RestoreTowersNextFrame(GameSaveData data)
    {
        yield return null;

        if (WaveManager.Instance != null)
            WaveManager.Instance.SetWave(data.lastCompletedWave);

        foreach (TowerSaveData towerData in data.towers)
        {
            GameObject prefab = GetPrefab(towerData.prefabName);
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab not found for {towerData.prefabName}");
                continue;
            }

            Vector3 pos = new Vector3(towerData.posX, towerData.posY, towerData.posZ);
            GameObject tower = Instantiate(prefab, pos, Quaternion.identity);

            yield return null;

            TowerUpgradeController upgradeCtrl = tower.GetComponent<TowerUpgradeController>();
            if (upgradeCtrl != null && towerData.upgradeLevels != null)
            {
                for (int i = 0; i < towerData.upgradeLevels.Length; i++)
                    for (int j = 0; j < towerData.upgradeLevels[i]; j++)
                        upgradeCtrl.PerformUpgradeWithoutCost(i);
            }

            Targeting targeting = tower.GetComponent<Targeting>();
            if (targeting != null)
                targeting.targetingMode = (TargetingMode)towerData.targetingMode;

            if (BuildManager.Instance != null)
                BuildManager.Instance.SetTowerLayer(tower);
        }
    }

    private GameObject GetPrefab(string name)
    {
        string trimmed = name.Trim();
        foreach (var mapping in towerPrefabs)
            if (string.Equals(mapping.towerName.Trim(), trimmed,
                System.StringComparison.OrdinalIgnoreCase))
                return mapping.prefab;

        Debug.LogError($"[GameLoader] No prefab mapping found for \"{name}\" — add it to the TowerPrefabs list in the Inspector.");
        return null;
    }
}
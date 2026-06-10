using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string SavePath => Application.persistentDataPath + "/saves/";
    private string MainFile(string scene) => SavePath + scene + ".json";
    private string BackupFile(string scene) => SavePath + scene + ".bak";

    private const int MAX_WAVE = 25;
    private const int MAX_MONEY = 999999;
    private const int MAX_LIVES = 50;
    private const int MAX_UPGRADE_LVL = 5;
    private const int MAX_TOWER_COUNT = 200;

    [System.Serializable]
    private class SaveFileWrapper
    {
        public string checksum; 
        public string dataJson;  
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);
    }

    // ── SAVE ─────────────────────────────────────────────────────────────────

    public void SaveGame()
    {
        try
        {
            GameSaveData data = CollectSaveData();

            string dataJson = JsonUtility.ToJson(data, true);
            string checksum = ComputeChecksum(dataJson);
            string wrapperJson = JsonUtility.ToJson(new SaveFileWrapper
            {
                checksum = checksum,
                dataJson = dataJson
            }, true);

            string main = MainFile(data.sceneName);
            string backup = BackupFile(data.sceneName);
            File.WriteAllText(main, wrapperJson);
            File.Copy(main, backup, overwrite: true);

            Debug.Log($"[SaveManager] Saved → {main}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] Save failed: {ex.Message}");
        }
    }
    public GameSaveData LoadGame(string sceneName)
    {
        GameSaveData data = TryLoad(MainFile(sceneName), sceneName, "main");

        if (data == null)
        {
            Debug.LogWarning("[SaveManager] Main save failed — trying backup.");
            data = TryLoad(BackupFile(sceneName), sceneName, "backup");
        }

        if (data == null)
            Debug.LogError("[SaveManager] Both main and backup are corrupt or missing. Starting fresh.");

        return data;
    }

    private GameSaveData TryLoad(string filePath, string expectedScene, string label)
    {
        if (!File.Exists(filePath))
        {
            Debug.Log($"[SaveManager] {label} not found: {filePath}");
            return null;
        }

        try
        {
            string wrapperJson = File.ReadAllText(filePath);
            SaveFileWrapper wrapper = JsonUtility.FromJson<SaveFileWrapper>(wrapperJson);

            if (wrapper == null || string.IsNullOrEmpty(wrapper.dataJson))
            {
                Debug.LogWarning($"[SaveManager] {label}: wrapper is empty or malformed.");
                return null;
            }

            string expectedChecksum = ComputeChecksum(wrapper.dataJson);
            if (wrapper.checksum != expectedChecksum)
            {
                Debug.LogWarning($"[SaveManager] {label}: checksum mismatch — file may be corrupt.");
                return null;
            }

            GameSaveData data = JsonUtility.FromJson<GameSaveData>(wrapper.dataJson);

            if (data == null)
            {
                Debug.LogWarning($"[SaveManager] {label}: data deserialized to null.");
                return null;
            }

            if (data.sceneName != expectedScene)
            {
                Debug.LogWarning($"[SaveManager] {label}: scene mismatch ({data.sceneName} vs {expectedScene}).");
                return null;
            }

            if (!ValidateAndRepair(data))
            {
                Debug.LogWarning($"[SaveManager] {label}: data failed validation.");
                return null;
            }

            Debug.Log($"[SaveManager] Loaded {label} — Wave {data.lastCompletedWave}");
            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SaveManager] {label} load exception: {ex.Message}");
            return null;
        }
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    public void DeleteSave(string sceneName)
    {
        TryDelete(MainFile(sceneName));
        TryDelete(BackupFile(sceneName));
    }

    private void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch (System.Exception ex)
        { Debug.LogWarning($"[SaveManager] Delete failed ({path}): {ex.Message}"); }
    }

    public bool SaveExists(string sceneName) =>
        File.Exists(MainFile(sceneName)) || File.Exists(BackupFile(sceneName));

    // ── VALIDATION ────────────────────────────────────────────────────────────


    private bool ValidateAndRepair(GameSaveData d)
    {

        if (d.towers == null) d.towers = new List<TowerSaveData>();
        if (d.abilities == null) d.abilities = new AbilitySaveData();

        d.lastCompletedWave = Mathf.Clamp(d.lastCompletedWave, 0, MAX_WAVE);
        d.currentMoney = Mathf.Clamp(d.currentMoney, 0, MAX_MONEY);
        d.currentLives = Mathf.Clamp(d.currentLives, 0, MAX_LIVES);
        d.enemiesDefeated = Mathf.Max(0, d.enemiesDefeated);
        d.timeElapsed = Mathf.Max(0f, d.timeElapsed);

        if (d.towers.Count > MAX_TOWER_COUNT)
        {
            Debug.LogWarning($"[SaveManager] Tower count {d.towers.Count} exceeds max — clearing tower list.");
            d.towers.Clear();
        }

        int targetingEnumMax = System.Enum.GetValues(typeof(TargetingMode)).Length - 1;
        List<TowerSaveData> validTowers = new List<TowerSaveData>();
        foreach (TowerSaveData t in d.towers)
        {
            if (string.IsNullOrEmpty(t.prefabName)) continue;

            if (t.upgradeLevels == null || t.upgradeLevels.Length != 3)
                t.upgradeLevels = new int[3];

            for (int i = 0; i < 3; i++)
                t.upgradeLevels[i] = Mathf.Clamp(t.upgradeLevels[i], 0, MAX_UPGRADE_LVL);

            t.targetingMode = Mathf.Clamp(t.targetingMode, 0, targetingEnumMax);
            validTowers.Add(t);
        }
        d.towers = validTowers;

        AbilitySaveData ab = d.abilities;
        ab.mineUpgradeLevel = Mathf.Clamp(ab.mineUpgradeLevel, 0, 3);
        ab.hedgehogUpgradeLevel = Mathf.Clamp(ab.hedgehogUpgradeLevel, 0, 3);
        ab.mortarUpgradeLevel = Mathf.Clamp(ab.mortarUpgradeLevel, 0, 3);
        ab.mineCooldown = Mathf.Max(0, ab.mineCooldown);
        ab.hedgehogCooldown = Mathf.Max(0, ab.hedgehogCooldown);
        ab.mortarCooldown = Mathf.Max(0, ab.mortarCooldown);
        ab.radarCooldown = Mathf.Max(0, ab.radarCooldown);

        return true;
    }

    // ── DATA COLLECTION ───────────────────────────────────────────────────────

    private GameSaveData CollectSaveData()
    {
        GameSaveData data = new GameSaveData();
        data.sceneName = SceneManager.GetActiveScene().name;

        if (WaveManager.Instance != null)
        {
            data.lastCompletedWave = WaveManager.Instance.isWaveActive
                ? WaveManager.Instance.currentWave - 1
                : WaveManager.Instance.currentWave;
        }

        if (GameManager.Instance != null)
        {
            data.currentMoney = GameManager.Instance.CurrentMoney;
            data.currentLives = GameManager.Instance.CurrentLives;
            data.enemiesDefeated = GameManager.Instance.EnemiesDefeated;
            data.timeElapsed = GameManager.Instance.TimeElapsed;
        }

        data.towers = SaveTowers();
        data.abilities = SaveAbilities();
        return data;
    }

    private List<TowerSaveData> SaveTowers()
    {
        List<TowerSaveData> towers = new List<TowerSaveData>();

        foreach (CombatTower tower in FindObjectsByType<CombatTower>(FindObjectsSortMode.None))
        {
            TowerSaveData t = new TowerSaveData();
            t.prefabName = tower.config.towerName;
            t.posX = tower.transform.position.x;
            t.posY = tower.transform.position.y;
            t.posZ = tower.transform.position.z;

            TowerUpgradeController uc = tower.GetComponent<TowerUpgradeController>();
            if (uc != null) t.upgradeLevels = uc.currentLevels;

            Targeting tgt = tower.GetComponent<Targeting>();
            if (tgt != null) t.targetingMode = (int)tgt.targetingMode;

            towers.Add(t);
        }

        foreach (RadarTower radar in FindObjectsByType<RadarTower>(FindObjectsSortMode.None))
        {
            TowerSaveData t = new TowerSaveData();
            t.prefabName = "Radar";
            t.posX = radar.transform.position.x;
            t.posY = radar.transform.position.y;
            t.posZ = radar.transform.position.z;

            TowerUpgradeController uc = radar.GetComponent<TowerUpgradeController>();
            if (uc != null) t.upgradeLevels = uc.currentLevels;

            towers.Add(t);
        }

        return towers;
    }

    private AbilitySaveData SaveAbilities()
    {
        AbilitySaveData data = new AbilitySaveData();
        if (AbilityManager.Instance == null) return data;

        data.mineUnlocked = AbilityManager.Instance.IsUnlocked(AbilityType.Mine);
        data.hedgehogUnlocked = AbilityManager.Instance.IsUnlocked(AbilityType.CzechHedgehog);
        data.mortarUnlocked = AbilityManager.Instance.IsUnlocked(AbilityType.Mortar);
        data.radarUnlocked = AbilityManager.Instance.IsUnlocked(AbilityType.Radar);

        data.mineUpgradeLevel = AbilityManager.Instance.GetUpgradeLevel(AbilityType.Mine);
        data.hedgehogUpgradeLevel = AbilityManager.Instance.GetUpgradeLevel(AbilityType.CzechHedgehog);
        data.mortarUpgradeLevel = AbilityManager.Instance.GetUpgradeLevel(AbilityType.Mortar);

        data.mineCooldown = AbilityManager.Instance.GetCooldownRemaining(AbilityType.Mine);
        data.hedgehogCooldown = AbilityManager.Instance.GetCooldownRemaining(AbilityType.CzechHedgehog);
        data.mortarCooldown = AbilityManager.Instance.GetCooldownRemaining(AbilityType.Mortar);
        data.radarCooldown = AbilityManager.Instance.GetCooldownRemaining(AbilityType.Radar);

        return data;
    }

    // ── CHECKSUM ─────────────────────────────────────────────────────────────

    private string ComputeChecksum(string data)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(data));
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
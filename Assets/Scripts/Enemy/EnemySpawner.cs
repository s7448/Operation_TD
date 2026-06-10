using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyEntry
    {
        public GameObject enemyPrefab;
        public int cost;
        public int maxPerWave;
        public int availableFromWave;
    }

    [Header("Path")]
    public PathController pathController;

    [Header("Enemy Settings")]
    public List<EnemyEntry> mechanizedEnemies = new List<EnemyEntry>();

    [Header("Infantry Settings")]
    public GameObject infantryPrefab;
    [SerializeField] private int baseInfantryCount = 10;
    [SerializeField] private float baseInfantryInterval = 1.5f;
    [SerializeField] private float intervalScalePerWave = 0.05f;

    [Header("Enemies to Spawn")]
    public List<GameObject> enemiesToSpawn = new List<GameObject>();

    [Header("Wave Settings")]
    public int currWave = 0;
    private int waveValue;
    [SerializeField] private int wavePointsValue = 10;

    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public float spawnInterval = 1f;
    private float spawnTimer;
    private bool spawningActive = false;

    private int infantryToSpawn = 0;
    private float infantryInterval = 1.5f;
    private float infantryTimer = 0f;

    void FixedUpdate()
    {
        if (!spawningActive) return;

        HandleMechanizedSpawning();
        HandleInfantrySpawning();

        if (enemiesToSpawn.Count == 0 && infantryToSpawn <= 0)
        {
            spawningActive = false;
            if (WaveManager.Instance != null)
                WaveManager.Instance.OnSpawningComplete();
        }
    }

    private void HandleMechanizedSpawning()
    {
        if (enemiesToSpawn.Count == 0) return;

        if (spawnTimer <= 0)
        {
            Vector3 rawPos = pathController != null && pathController.GetSpawnPoint() != null
                ? pathController.GetSpawnPoint().position
                : spawnPoint.position;

            GameObject prefab = enemiesToSpawn[0];
            enemiesToSpawn.RemoveAt(0);
            spawnTimer = spawnInterval;

            float heightOffset = prefab.GetComponent<EnemyController>()?.stats?.spawnHeightOffset ?? 0f;
            Vector3 spawnPos = rawPos + Vector3.up * heightOffset;

            GameObject spawnedEnemy = ObjectPool.Instance != null
                ? ObjectPool.Instance.Get(prefab, spawnPos, Quaternion.identity)
                : Instantiate(prefab, spawnPos, Quaternion.identity);

            EnemyController enemyCtrl = spawnedEnemy.GetComponent<EnemyController>();
            if (enemyCtrl != null && pathController != null)
                enemyCtrl.Prepare(pathController.GetWaypoints());

            if (WaveManager.Instance != null)
                WaveManager.Instance.OnEnemySpawned();
        }
        else
        {
            spawnTimer -= Time.fixedDeltaTime;
        }
    }

    private void HandleInfantrySpawning()
    {
        if (infantryToSpawn <= 0) return;

        if (infantryTimer <= 0)
        {
            Vector3 rawPos = pathController != null && pathController.GetSpawnPoint() != null
                ? pathController.GetSpawnPoint().position
                : spawnPoint.position;

            float heightOffset = infantryPrefab.GetComponent<EnemyController>()?.stats?.spawnHeightOffset ?? 0f;
            Vector3 spawnPos = rawPos + Vector3.up * heightOffset;

            GameObject spawnedEnemy = ObjectPool.Instance != null
                ? ObjectPool.Instance.Get(infantryPrefab, spawnPos, Quaternion.identity)
                : Instantiate(infantryPrefab, spawnPos, Quaternion.identity);
            infantryToSpawn--;
            infantryTimer = infantryInterval;

            EnemyController enemyCtrl = spawnedEnemy.GetComponent<EnemyController>();
            if (enemyCtrl != null && pathController != null)
                enemyCtrl.Prepare(pathController.GetWaypoints());

            if (WaveManager.Instance != null)
                WaveManager.Instance.OnEnemySpawned();
        }
        else
        {
            infantryTimer -= Time.fixedDeltaTime;
        }
    }

    public void PrecalculateWave(int waveNumber, int totalSpawners = 1)
    {
        currWave = waveNumber;

        int totalInfantry = baseInfantryCount + (currWave * 2);
        int infantryShare = Mathf.CeilToInt((float)totalInfantry / totalSpawners);
        if (infantryShare % 2 != 0) infantryShare++;
        infantryToSpawn = infantryShare;
        infantryInterval = Mathf.Max(0.2f, baseInfantryInterval - (currWave * intervalScalePerWave));
        infantryTimer = 0f;

        int totalBudget = Mathf.RoundToInt(wavePointsValue * Mathf.Pow(currWave, 1.5f));
        waveValue = Mathf.RoundToInt((float)totalBudget / totalSpawners);
        enemiesToSpawn.Clear();

        Dictionary<int, int> enemyCount = new Dictionary<int, int>();
        for (int i = 0; i < mechanizedEnemies.Count; i++)
            enemyCount[i] = 0;

        int safetyCounter = 0;
        while (waveValue > 0 && safetyCounter < 1000)
        {
            safetyCounter++;
            List<int> availableEnemies = new List<int>();

            for (int i = 0; i < mechanizedEnemies.Count; i++)
            {
                bool withinBudget = mechanizedEnemies[i].cost <= waveValue;
                bool availableThisWave = mechanizedEnemies[i].availableFromWave <= currWave;
                int scaledMax = GetScaledMaxPerWave(mechanizedEnemies[i], currWave);
                bool withinLimit = scaledMax == 0 || enemyCount[i] < scaledMax;

                if (withinBudget && availableThisWave && withinLimit)
                    availableEnemies.Add(i);
            }

            if (availableEnemies.Count == 0) break;

            int randomIndex = availableEnemies[Random.Range(0, availableEnemies.Count)];
            enemiesToSpawn.Add(mechanizedEnemies[randomIndex].enemyPrefab);
            enemyCount[randomIndex]++;
            waveValue -= mechanizedEnemies[randomIndex].cost;
        }
    }

    public void StartSpawning()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.RegisterSpawner();
        spawningActive = true;
    }

    public Dictionary<string, int> GetUpcomingIntel()
    {
        Dictionary<string, int> intel = new Dictionary<string, int>();

        if (infantryToSpawn > 0)
            intel["Infantry"] = infantryToSpawn;

        foreach (GameObject prefab in enemiesToSpawn)
        {
            if (prefab == null) continue;
            string cleanName = prefab.name.Replace("Prefab", "").Replace("(Clone)", "").Trim();
            if (intel.ContainsKey(cleanName)) intel[cleanName]++;
            else intel[cleanName] = 1;
        }

        return intel;
    }

    private int GetScaledMaxPerWave(EnemyEntry enemy, int currentWave)
    {
        if (enemy.maxPerWave == 0) return 0;
        int waveBonus = (currentWave - enemy.availableFromWave) / 5;
        return enemy.maxPerWave + waveBonus;
    }
}
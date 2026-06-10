using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<EnemySpawner> enemySpawners = new List<EnemySpawner>();
    public int maxWaves => _maxWaves;
    [SerializeField] private int _maxWaves = 15;
    private int activeSpawners = 0;

    public bool isWaveActive { get; private set; } = false;
    public int currentWave { get; private set; } = 0;
    public int enemiesRemaining { get; private set; } = 0;
    public bool canStartNextWave { get; private set; } = true;
    private bool isSpawning = false;

    [Header("Wave Completion Bonus")]
    [Tooltip("Base money reward given at the end of every wave.")]
    [SerializeField] private int baseWaveBonus = 75;
    [Tooltip("Percentage increase applied per wave (0.08 = 8% per wave).")]
    [SerializeField] private float waveBonusScaling = 0.08f;

    public event System.Action OnWaveEnded;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        foreach (var spawner in enemySpawners)
            spawner.PrecalculateWave(1, enemySpawners.Count);
    }

    void OnEnable()
    {
        EnemyController.OnEnemyDeath += HandleEnemyDeath;
        EnemyController.OnEnemyEscaped += HandleEnemyEscaped;
    }

    void OnDisable()
    {
        EnemyController.OnEnemyDeath -= HandleEnemyDeath;
        EnemyController.OnEnemyEscaped -= HandleEnemyEscaped;
    }

    private void HandleEnemyDeath(EnemyController enemy) => DecrementEnemies();
    private void HandleEnemyEscaped(EnemyController enemy) => DecrementEnemies();

    private void DecrementEnemies()
    {
        enemiesRemaining = Mathf.Max(0, enemiesRemaining - 1);
        CheckWaveEnd();
    }

    private void CheckWaveEnd()
    {
        if (enemiesRemaining <= 0 && isWaveActive && !isSpawning && activeSpawners <= 0)
            EndWave();
    }

    public void StartWave()
    {
        if (isWaveActive || !canStartNextWave) return;
        if (enemiesRemaining > 0) return;

        isWaveActive = true;
        canStartNextWave = false;
        enemiesRemaining = 0;
        activeSpawners = 0;
        isSpawning = true;
        currentWave++;

        foreach (var spawner in enemySpawners)
            spawner.PrecalculateWave(currentWave, enemySpawners.Count);

        if (UIManager.Instance != null)
            UIManager.Instance.OnWaveStateChanged(isWaveActive, currentWave);

        foreach (var spawner in enemySpawners)
            spawner.StartSpawning();
    }

    public void OnEnemySpawned() => enemiesRemaining++;
    public void RegisterSpawner() => activeSpawners++;

    public void OnSpawningComplete()
    {
        activeSpawners = Mathf.Max(0, activeSpawners - 1);
        if (activeSpawners <= 0) isSpawning = false;
        CheckWaveEnd();
    }

    private void EndWave()
    {
        isWaveActive = false;
        canStartNextWave = true;

        int bonus = Mathf.RoundToInt(baseWaveBonus * Mathf.Pow(1f + waveBonusScaling, currentWave));
        if (GameManager.Instance != null)
            GameManager.Instance.AddMoney(bonus);

        OnWaveEnded?.Invoke();

        if (currentWave >= _maxWaves)
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.DeleteSave(SceneManager.GetActiveScene().name);
            if (UIManager.Instance != null)
                UIManager.Instance.ShowVictory();
            return;
        }

        int nextWave = currentWave + 1;
        foreach (var spawner in enemySpawners)
            spawner.PrecalculateWave(nextWave, enemySpawners.Count);

        if (UIManager.Instance != null)
            UIManager.Instance.OnWaveStateChanged(isWaveActive, currentWave);
    }

    public void SetWave(int wave)
    {
        currentWave = wave;
        int upcoming = Mathf.Min(wave + 1, _maxWaves);
        foreach (var spawner in enemySpawners)
            spawner.PrecalculateWave(upcoming, enemySpawners.Count);
        if (UIManager.Instance != null)
            UIManager.Instance.OnWaveStateChanged(false, currentWave);
    }

    public List<EnemySpawner> GetSpawners() => enemySpawners;

    public int GetWaveBonus(int wave) =>
        Mathf.RoundToInt(baseWaveBonus * Mathf.Pow(1f + waveBonusScaling, wave));
}
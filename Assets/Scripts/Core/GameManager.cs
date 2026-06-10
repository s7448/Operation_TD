using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Starting Stats")]
    [SerializeField] private int startingMoney = 500;
    [SerializeField] private int startingLives = 20;

    public int CurrentMoney { get; private set; }
    public int CurrentLives { get; private set; }
    public bool IsGameOver { get; private set; } = false;

    public int EnemiesDefeated { get; private set; } = 0;
    public float TimeElapsed { get; private set; } = 0f;
    public bool IsGameRunning { get; private set; } = false;

    public void SetEnemiesDefeated(int amount) => EnemiesDefeated = amount;
    public void SetTimeElapsed(float time) => TimeElapsed = time;

    public event Action<int> OnMoneyChanged;
    public event Action<int> OnLivesChanged;
    public event Action OnGameOver;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        if (IsGameRunning && !IsGameOver)
            TimeElapsed += Time.deltaTime;
    }


    void OnEnable()
    {
        EnemyController.OnEnemyDeath += HandleEnemyReward;
        EnemyController.OnLivesLost += ReduceLives;
    }

    void OnDisable()
    {
        EnemyController.OnEnemyDeath -= HandleEnemyReward;
        EnemyController.OnLivesLost -= ReduceLives;
    }


    private void HandleEnemyReward(EnemyController enemy)
    {
        if (enemy != null && enemy.stats != null)
        {
            AddMoney(enemy.stats.goldReward);
            EnemiesDefeated++;
        }
    }

    public void ReduceLives(int amount)
    {
        if (IsGameOver) return;

        CurrentLives -= amount;
        if (CurrentLives < 0) CurrentLives = 0;

        OnLivesChanged?.Invoke(CurrentLives);
        Debug.Log($"Lives Lost! Remaining: {CurrentLives}");

        if (CurrentLives <= 0)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        IsGameRunning = true;
        TimeElapsed = 0f;
        EnemiesDefeated = 0;

        CurrentMoney = startingMoney;
        OnMoneyChanged?.Invoke(CurrentMoney);

        CurrentLives = startingLives;
        OnLivesChanged?.Invoke(CurrentLives);
    }
    private void EndGame()
    {
        IsGameOver = true;
        OnGameOver?.Invoke();
        Debug.Log("GAME OVER");
    }


    public bool CanAfford(int cost)
    {
        return CurrentMoney >= cost;
    }

    public bool SpendMoney(int amount)
    {
        if (!CanAfford(amount))
        {
            Debug.Log($"Not enough money! Need {amount}, have {CurrentMoney}");
            return false;
        }

        CurrentMoney -= amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        Debug.Log($"Spent {amount}. Remaining: {CurrentMoney}");
        return true;
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
        Debug.Log($"Gained {amount}. Total: {CurrentMoney}");
    }

    public void SetMoney(int amount)
    {
        CurrentMoney = amount;
        OnMoneyChanged?.Invoke(CurrentMoney);
    }

    public void SetLives(int amount)
    {
        CurrentLives = amount;
        OnLivesChanged?.Invoke(CurrentLives);
    }

}
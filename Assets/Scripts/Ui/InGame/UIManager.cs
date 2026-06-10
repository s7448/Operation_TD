using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Stats UI")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Result Screen")]
    [SerializeField] private LevelResultScreen levelResultScreen;

    [Header("Wave Button")]
    [SerializeField] private HoverButton startWaveButton;

    [Header("Pause Button")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseBackground;
    [SerializeField] private Image pauseIcon;

    private bool isPaused = false;

    private static readonly Color ColorWhite = Color.white;
    private static readonly Color ColorBlack = Color.black;

    // -------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SetupWaveButton();
        SetupPauseButton();
        UpdateWaveUI(false, 0);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateMoneyUI;
            GameManager.Instance.OnLivesChanged += UpdateLivesUI;
            GameManager.Instance.OnGameOver += ShowGameOver;

            UpdateMoneyUI(GameManager.Instance.CurrentMoney);
            UpdateLivesUI(GameManager.Instance.CurrentLives);
        }
        else
            Debug.LogError("UIManager: GameManager not found");
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateMoneyUI;
            GameManager.Instance.OnLivesChanged -= UpdateLivesUI;
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }


    void UpdateMoneyUI(int amount)
    {
        if (moneyText != null) moneyText.text = $"{amount} $";
    }

    void UpdateLivesUI(int amount)
    {
        if (livesText != null) livesText.text = $"{amount}";
    }



    void ShowGameOver()
    {
        if (levelResultScreen != null)
            levelResultScreen.ShowDefeat();
    }

    public void ShowVictory()
    {
        if (levelResultScreen != null)
            levelResultScreen.ShowVictory();
    }

    // -------------------------------------------------------
    // WAVE BUTTON

    private void SetupWaveButton()
    {
        if (startWaveButton == null) return;

        startWaveButton.button.transition = Selectable.Transition.None;
        startWaveButton.button.onClick.RemoveAllListeners();
        startWaveButton.button.onClick.AddListener(OnStartWaveButtonPressed);
    }

    private void OnStartWaveButtonPressed()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.StartWave();
    }

    public void OnWaveStateChanged(bool isWaveActive, int currentWave)
    {
        UpdateWaveUI(isWaveActive, currentWave);
    }

    private void UpdateWaveUI(bool isWaveActive, int currentWave)
    {
        if (startWaveButton != null)
            startWaveButton.SetInteractable(!isWaveActive);

        if (waveText != null)
            waveText.text = $"{currentWave}";
    }

    // -------------------------------------------------------
    // PAUSE BUTTON

    private void SetupPauseButton()
    {
        if (pauseButton == null) return;

        pauseButton.transition = Selectable.Transition.None;
        pauseButton.onClick.RemoveAllListeners();
        pauseButton.onClick.AddListener(OnPauseButtonPressed);

        var trigger = pauseButton.gameObject.GetComponent<EventTrigger>()
                      ?? pauseButton.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        var onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onEnter.callback.AddListener(_ => SetPauseButtonHover(true));
        trigger.triggers.Add(onEnter);

        var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onExit.callback.AddListener(_ => SetPauseButtonHover(false));
        trigger.triggers.Add(onExit);
    }

    private void OnPauseButtonPressed()
    {
        if (PauseManager.Instance != null)
        {
            if (PauseManager.Instance.IsPaused)
                PauseManager.Instance.Resume();
            else
                PauseManager.Instance.Pause();
        }
    }

    private void SetPauseButtonHover(bool hovering)
    {
        if (isPaused) return;

        if (pauseBackground != null)
            pauseBackground.color = hovering ? ColorBlack : ColorWhite;
        if (pauseIcon != null)
            pauseIcon.color = hovering ? ColorWhite : ColorBlack;
    }
}
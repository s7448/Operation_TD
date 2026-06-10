using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Button speedButton;

    [Tooltip("Optional — assign if your button uses a text label (e.g. '2x')")]
    [SerializeField] private TextMeshProUGUI speedLabel;

    [Tooltip("Optional — assign if your button uses an icon image")]
    [SerializeField] private Image speedIcon;

    [Tooltip("One sprite per speed step. Leave empty if using text label only.")]
    [SerializeField] private Sprite[] speedSprites;

    [Header("Speed Steps")]
    [SerializeField] private float[] speedSteps = { 1f, 2f, 3f };

    private int currentStepIndex = 0;
    public float CurrentSpeed => speedSteps[currentStepIndex];

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        speedButton.onClick.AddListener(CycleSpeed);
        Refresh();
    }

    // ── PauseManager hooks ────────────────────────────────────────────────────

    public void OnGamePaused()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void OnGameResumed()
    {
        isPaused = false;
        Time.timeScale = CurrentSpeed;
    }

    // ── Cycling ───────────────────────────────────────────────────────────────

    private void CycleSpeed()
    {
        currentStepIndex = (currentStepIndex + 1) % speedSteps.Length;
        if (!isPaused) Time.timeScale = CurrentSpeed;
        Refresh();
    }

    private void Refresh()
    {
        if (speedLabel != null)
            speedLabel.text = $"{CurrentSpeed:0}x";

        if (speedIcon != null && speedSprites != null &&
            currentStepIndex < speedSprites.Length &&
            speedSprites[currentStepIndex] != null)
            speedIcon.sprite = speedSprites[currentStepIndex];
    }

    // ── Reset ─────────────────────────────────────────────────────────────────


    public void ResetSpeed()
    {
        currentStepIndex = 0;
        if (!isPaused) Time.timeScale = 1f;
        Refresh();
    }
}
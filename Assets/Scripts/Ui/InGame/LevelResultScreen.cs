using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LevelResultScreen : MonoBehaviour
{
    [Header("Modal")]
    [SerializeField] private Image modalBackground;
    [SerializeField] private Image overlayBackground;

    [Header("Header")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private Sprite victorySprite;
    [SerializeField] private Sprite defeatSprite;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Image headerBorder;

    [Header("Stat Labels")]
    [SerializeField] private TextMeshProUGUI wavesLabel;
    [SerializeField] private TextMeshProUGUI enemiesLabel;
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private Image[] dividers;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI reportTitleText;
    [SerializeField] private TextMeshProUGUI wavesText;
    [SerializeField] private TextMeshProUGUI enemiesText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private Image statsContainerBorder;

    [Header("Footer")]
    [SerializeField] private Image footerBorder;
    [SerializeField] private Button retryButton;
    [SerializeField] private Image retryBackground;
    [SerializeField] private TextMeshProUGUI retryText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Image mainMenuBackground;
    [SerializeField] private TextMeshProUGUI mainMenuText;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    [Header("Animation")]
    [SerializeField] private RectTransform modalBox;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float slideOffset = 100f;

    private static readonly Color VictoryBg = Color.white;
    private static readonly Color VictoryText = Color.black;
    private static readonly Color VictoryBorder = Color.black;
    private static readonly Color DefeatBg = Color.black;
    private static readonly Color DefeatText = Color.white;
    private static readonly Color DefeatBorder = Color.white;

    private CanvasGroup canvasGroup;
    private bool isVictory;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Start()
    {
        retryButton.onClick.AddListener(OnRetry);
        mainMenuButton.onClick.AddListener(OnMainMenu);


        AddHoverEffect(retryButton, retryBackground, retryText);
        AddHoverEffect(mainMenuButton, mainMenuBackground, mainMenuText);
    }

    // -------------------------------------------------------
    // SHOW

    public void ShowVictory()
    {
        isVictory = true;
        PopulateStats();
        ApplyTheme(true);
        StartCoroutine(AnimateIn());
    }

    public void ShowDefeat()
    {
        isVictory = false;
        PopulateStats();
        ApplyTheme(false);
        StartCoroutine(AnimateIn());
    }

    // -------------------------------------------------------
    // THEME

    private void ApplyTheme(bool victory)
    {
        Color bg = victory ? VictoryBg : DefeatBg;
        Color text = victory ? VictoryText : DefeatText;
        Color border = victory ? VictoryBorder : DefeatBorder;

        modalBackground.color = bg;

        titleText.text = victory ? "VICTORY" : "DEFEAT";
        titleText.color = text;

        subtitleText.text = victory ? "MISSION ACCOMPLISHED" : "MISSION FAILED";
        subtitleText.color = new Color(text.r, text.g, text.b, 0.8f);

        resultIcon.sprite = victory ? victorySprite : defeatSprite;
        resultIcon.color = text;


        if (reportTitleText) reportTitleText.color = text;
        if (wavesText) wavesText.color = text;
        if (enemiesText) enemiesText.color = text;
        if (timeText) timeText.color = text;


        if (headerBorder) headerBorder.color = border;
        if (footerBorder) footerBorder.color = border;
        if (statsContainerBorder) statsContainerBorder.color = border;


        retryBackground.color = victory ? Color.white : Color.black;
        retryText.color = victory ? Color.black : Color.white;
        mainMenuBackground.color = victory ? Color.white : Color.black;
        mainMenuText.color = victory ? Color.black : Color.white;

        if (wavesLabel) wavesLabel.color = text;
        if (enemiesLabel) enemiesLabel.color = text;
        if (timeLabel) timeLabel.color = text;

        if (dividers != null)
            foreach (Image d in dividers)
                if (d != null) d.color = new Color(text.r, text.g, text.b, 0.3f);
    }

    // -------------------------------------------------------
    // STATS

    private void PopulateStats()
    {
        if (GameManager.Instance == null) return;

        int currentWave = WaveManager.Instance != null ? WaveManager.Instance.currentWave : 0;
        int maxWaves = WaveManager.Instance != null ? WaveManager.Instance.maxWaves : 0;

        if (wavesText != null)
            wavesText.text = $"{currentWave} / {maxWaves}";

        if (enemiesText != null)
            enemiesText.text = GameManager.Instance.EnemiesDefeated.ToString();

        if (timeText != null)
            timeText.text = FormatTime(GameManager.Instance.TimeElapsed);
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return $"{mins:00}:{secs:00}";
    }

    // -------------------------------------------------------
    // BUTTONS

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    // -------------------------------------------------------
    // ANIMATION

    private IEnumerator AnimateIn()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Vector2 startPos = new Vector2(0, -slideOffset);
        Vector2 endPos = Vector2.zero;
        modalBox.anchoredPosition = startPos;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            modalBox.anchoredPosition = Vector2.Lerp(startPos, endPos,
                Mathf.SmoothStep(0f, 1f, t));

            yield return null;
        }

        canvasGroup.alpha = 1f;
        modalBox.anchoredPosition = endPos;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    // -------------------------------------------------------
    // HOVER

    private void AddHoverEffect(Button button, Image background, TextMeshProUGUI text)
    {
        var hover = button.gameObject.GetComponent<HoverButton>() ??
                    button.gameObject.AddComponent<HoverButton>();

        hover.backgroundImage = background;
        hover.labelText = text;
        hover.button = button;

        if (isVictory)
        {
            hover.normalBgColor = Color.white;
            hover.normalContentColor = Color.black;
            hover.hoverBgColor = Color.black;
            hover.hoverContentColor = Color.white;
        }
        else
        {
            hover.normalBgColor = Color.black;
            hover.normalContentColor = Color.white;
            hover.hoverBgColor = Color.white;
            hover.hoverContentColor = Color.black;
        }
    }
}
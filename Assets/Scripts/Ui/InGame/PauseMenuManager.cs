using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private CanvasGroup pauseMenuGroup;
    [SerializeField] private CanvasGroup settingsGroup;
    [SerializeField] private CanvasGroup handbookGroup;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button handbookButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Settings/Handbook Back Buttons")]
    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button handbookBackButton;

    [Header("Scene Names")]
    [SerializeField] private string menuScene = "Menu";

    private bool isPaused = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        HideGroup(pauseMenuGroup);
        HideGroup(settingsGroup);
        HideGroup(handbookGroup);

        resumeButton.onClick.AddListener(Resume);
        handbookButton.onClick.AddListener(OpenHandbook);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(QuitAndSave);

        if (settingsBackButton != null)
            settingsBackButton.onClick.AddListener(CloseSettings);
        if (handbookBackButton != null)
            handbookBackButton.onClick.AddListener(CloseHandbook);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    // -------------------------------------------------------
    // PAUSE / RESUME

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        ShowGroup(pauseMenuGroup);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HideGroup(pauseMenuGroup);
        HideGroup(settingsGroup);
        HideGroup(handbookGroup);
    }

    public bool IsPaused => isPaused;

    // -------------------------------------------------------
    // HANDBOOK

    private void OpenHandbook()
    {

        HideGroup(pauseMenuGroup);
        ShowGroup(handbookGroup);
    }

    private void CloseHandbook()
    {
        Debug.Log($"OpenHandbook — handbookGroup null: {handbookGroup == null}");
        HideGroup(handbookGroup);
        ShowGroup(pauseMenuGroup);
    }

    // -------------------------------------------------------
    // SETTINGS

    private void OpenSettings()
    {
        Debug.Log($"OpenSettings — settingsGroup null: {settingsGroup == null}");
        HideGroup(pauseMenuGroup);
        ShowGroup(settingsGroup);
    }

    private void CloseSettings()
    {
        HideGroup(settingsGroup);
        ShowGroup(pauseMenuGroup);
    }

    // -------------------------------------------------------
    // QUIT

    private void QuitAndSave()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        isPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuScene);
    }

    // -------------------------------------------------------
    // HELPERS

    private void ShowGroup(CanvasGroup group)
    {
        if (group == null) return;
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    private void HideGroup(CanvasGroup group)
    {
        if (group == null) return;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
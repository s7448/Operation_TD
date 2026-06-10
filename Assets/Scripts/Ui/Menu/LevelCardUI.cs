using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI difficultyValueText;
    public TextMeshProUGUI wavesValueText;
    public Image cardBackground;

    [Header("Data")]
    public string sceneName;

    [Header("Buttons")]
    [SerializeField] private Button startMissionButton;
    [SerializeField] private Button loadMissionButton;

    private static readonly Color NormalColor = Color.white;
    private static readonly Color HoverColor = new Color(0.9f, 0.9f, 0.9f);

    private bool isSelected = false;

    void Start()
    {
        cardBackground.color = NormalColor;

        if (startMissionButton != null)
            startMissionButton.onClick.AddListener(StartLevel);

        if (loadMissionButton != null)
        {
            loadMissionButton.onClick.AddListener(LoadLevel);
            UpdateLoadButton();
        }
    }


    public void Setup(string title, string desc, string difficulty, string waves)
    {
        levelNameText.text = title;
        descriptionText.text = desc;
        difficultyValueText.text = difficulty;
        wavesValueText.text = waves;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            cardBackground.color = HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            cardBackground.color = NormalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Object.FindFirstObjectByType<LevelManager>().SelectLevel(this);
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
        cardBackground.color = state ? HoverColor : NormalColor;
        transform.localScale = state ? Vector3.one * 1.05f : Vector3.one;
    }

    private void UpdateLoadButton()
    {
        if (loadMissionButton == null) return;

        string savePath = Application.persistentDataPath + "/saves/" + sceneName + ".json";
        bool saveExists = System.IO.File.Exists(savePath);
        loadMissionButton.gameObject.SetActive(saveExists);
    }

    private void StartLevel()
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneTransitionData.ShowLevelSelect = false;
        SceneTransitionData.LoadSave = false;
        SceneManager.LoadScene(sceneName);
    }



    private void LoadLevel()
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        SceneTransitionData.LoadSave = true;
        SceneTransitionData.SaveSceneName = sceneName;
        SceneManager.LoadScene(sceneName);
    }
}
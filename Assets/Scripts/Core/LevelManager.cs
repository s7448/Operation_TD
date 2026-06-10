using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LevelData
{
    public string levelName;
    public string description;
    public string difficulty;
    public string waves;
    public string sceneName;
}


public class LevelManager : MonoBehaviour
{
    public GameObject levelCardPrefab;
    public Transform gridParent;
    public List<LevelData> allLevels;

    private LevelCardUI currentSelectedCard;
    private string selectedSceneName;

    void Start()
    {
        GenerateLevels();
    }

    void GenerateLevels()
    {
        foreach (LevelData level in allLevels)
        {
            GameObject newCard = Instantiate(levelCardPrefab, gridParent);
            LevelCardUI cardUI = newCard.GetComponent<LevelCardUI>();
            cardUI.Setup(level.levelName, level.description, level.difficulty, level.waves);
            cardUI.sceneName = level.sceneName;
        }
    }

    public void SelectLevel(LevelCardUI clickedCard)
    {
        if (currentSelectedCard != null)
            currentSelectedCard.SetSelected(false);

        currentSelectedCard = clickedCard;
        currentSelectedCard.SetSelected(true);
        selectedSceneName = clickedCard.sceneName;
    }

    private void LoadSelectedLevel()
    {
        if (string.IsNullOrEmpty(selectedSceneName)) return;
        SceneTransitionData.ShowLevelSelect = false;
        SceneManager.LoadScene(selectedSceneName);
    }
}
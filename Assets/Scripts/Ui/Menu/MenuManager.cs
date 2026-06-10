using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelMainMenu;
    public GameObject panelSettings;
    public GameObject panelHandbook;
    public GameObject panelLevelSelector;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetAllPanelsInactive();
        panelMainMenu.SetActive(true);
    }

    public void ShowSettings()
    {
        SetAllPanelsInactive();
        panelSettings.SetActive(true);
    }

    public void ShowHandbook()
    {
        SetAllPanelsInactive();
        panelHandbook.SetActive(true);
    }

    public void ShowLevelSelector()
    {
        SetAllPanelsInactive();

        if (panelLevelSelector != null)
        {
            panelLevelSelector.SetActive(true);
        }
    }

    private void SetAllPanelsInactive()
    {
        if (panelMainMenu != null) panelMainMenu.SetActive(false);
        if (panelSettings != null) panelSettings.SetActive(false);
        if (panelHandbook != null) panelHandbook.SetActive(false);
        if (panelLevelSelector != null) panelLevelSelector.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Exiting!");
        Application.Quit();
    }
}
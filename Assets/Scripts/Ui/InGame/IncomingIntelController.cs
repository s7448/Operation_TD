using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class IncomingIntelController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public GameObject dropdownPanel;
    public TextMeshProUGUI headerText;
    public Transform enemyListContainer;

    [Tooltip("A simple prefab containing a TextMeshPro text element")]
    public GameObject enemyRowPrefab;

    void Start()
    {
        dropdownPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateIntelDisplay();
        dropdownPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dropdownPanel.SetActive(false);
    }

    private void UpdateIntelDisplay()
    {
        if (WaveManager.Instance == null) return;

        int upcomingWave = WaveManager.Instance.currentWave + 1;

        if (upcomingWave > WaveManager.Instance.maxWaves)
        {
            headerText.text = "ALL WAVES CLEARED";
            ClearList();
            return;
        }

        headerText.text = $"WAVE {upcomingWave} PREVIEW";
        ClearList();

        Dictionary<string, int> combinedIntel = new Dictionary<string, int>();

        foreach (var spawner in WaveManager.Instance.GetSpawners())
        {
            var spawnerIntel = spawner.GetUpcomingIntel();
            foreach (var kvp in spawnerIntel)
            {
                if (combinedIntel.ContainsKey(kvp.Key))
                    combinedIntel[kvp.Key] += kvp.Value;
                else
                    combinedIntel[kvp.Key] = kvp.Value;
            }
        }

        foreach (var kvp in combinedIntel)
        {
            GameObject row = Instantiate(enemyRowPrefab, enemyListContainer);
            TextMeshProUGUI textComp = row.GetComponentInChildren<TextMeshProUGUI>();

            if (textComp != null)
            {
                textComp.text = $"{kvp.Key}  <color=#555555>x{kvp.Value}</color>";
            }
        }
    }

    private void ClearList()
    {
        foreach (Transform child in enemyListContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
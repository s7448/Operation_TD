using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class UnitIconSelect : MonoBehaviour
{
    public Image backgroundImage;
    public Image iconImage;
    public TextMeshProUGUI labelText;

    public EnemyStats enemyData;
    public TowerConfig towerData;

    [Header("Colors")]
    public Color normalBgColor = Color.white;
    public Color selectedBgColor = Color.black;

    public Color normalTextColor = Color.black;
    public Color selectedTextColor = Color.white;

    private bool isSelected = false;

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        backgroundImage.color = isSelected ? selectedBgColor : normalBgColor;

        if (labelText != null)
            labelText.color = isSelected ? selectedTextColor : normalTextColor;

        if (iconImage != null)
            iconImage.color = isSelected ? selectedTextColor : normalTextColor; 
    }

    public void OnIconClicked()
    {
        if (enemyData != null)
        {
            HandbookManager.Instance.UpdateEnemyDetails(this, enemyData);
        }
        else if (towerData != null)
        {
            HandbookManager.Instance.UpdateTurretDetails(this, towerData);
        }
    }
}
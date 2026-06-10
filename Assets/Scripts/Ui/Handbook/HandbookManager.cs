using UnityEngine;
using TMPro;

public class HandbookManager : MonoBehaviour
{
    public static HandbookManager Instance;

    [Header("Initialization")]
    public UnitIconSelect defaultEnemyButton;
    public UnitIconSelect defaultTurretButton;


    private UnitIconSelect currentSelectedEnemy;
    private UnitIconSelect currentSelectedTurret;

    [Header("Left Panel (Enemies)")]
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyDescText;
    public UnityEngine.UI.Image enemyLargeIcon;
    public TextMeshProUGUI[] enemyStatValues; 
    public EnemyDetailsPanel hardnessScript;

    [Header("Right Panel (Turrets)")]
    public TextMeshProUGUI turretNameText;
    public TextMeshProUGUI turretDescText;
    public UnityEngine.UI.Image turretLargeIcon;
    public TextMeshProUGUI[] turretStatValues; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (defaultEnemyButton != null)
        {
            defaultEnemyButton.OnIconClicked();
        }

        if (defaultTurretButton != null)
        {
            defaultTurretButton.OnIconClicked();
        }
    }

    public void UpdateEnemyDetails(UnitIconSelect iconScript, EnemyStats data)
    {
        Debug.Log($"UpdateEnemyDetails — enemy: {data.enemyName}, icon: {data.icon}");
        if (currentSelectedEnemy != null)
        {
            currentSelectedEnemy.SetSelected(false);
        }
        currentSelectedEnemy = iconScript;
        currentSelectedEnemy.SetSelected(true);

        enemyNameText.text = data.enemyName.ToUpper();
        enemyDescText.text = data.description;
        enemyLargeIcon.sprite = data.icon;

        enemyStatValues[0].text = data.maxHealth.ToString();
        enemyStatValues[1].text = data.movementSpeed.ToString();
        enemyStatValues[2].text = data.armorValue.ToString();
        enemyStatValues[3].text = "$" + data.goldReward.ToString();
        enemyStatValues[4].text = data.firstWaveAppear.ToString();
        enemyStatValues[5].text = data.damageToPlayer.ToString();

        if (hardnessScript != null)
        {
            hardnessScript.targetFillAmount = data.hardness ;
        }
    }

    public void UpdateTurretDetails(UnitIconSelect iconScript, TowerConfig data)
    {
        if (currentSelectedTurret != null)
        {
            currentSelectedTurret.SetSelected(false);
        }
        currentSelectedTurret = iconScript;
        currentSelectedTurret.SetSelected(true);

        turretNameText.text = data.towerName.ToUpper();
        turretDescText.text = data.description;
        turretLargeIcon.sprite = data.icon;

        turretStatValues[3].text = data.range.ToString();
        turretStatValues[4].text = data.fireRate.ToString() + "/s";
        turretStatValues[5].text = "$" + data.cost.ToString();

        if (data is CombatTowerConfig combat)
        {
            turretStatValues[0].text = combat.softAttack.ToString();
            turretStatValues[1].text = combat.hardAttack.ToString();
            turretStatValues[2].text = combat.piercing.ToString();
        }
        else if (data is SupportTowerConfig support)
        {
            turretStatValues[0].text = support.supportType.ToString();
            turretStatValues[1].text = "x" + support.multiplier.ToString();
            turretStatValues[2].text = support.effectDuration > 0 ? support.effectDuration + "s" : "Infinite";
        }
    }
}
using UnityEngine;

public enum UpgradeStat
{
    SoftAttack,
    HardAttack,
    Piercing,
    Range,
    FireRate
}

[System.Serializable]
public struct UpgradeStep
{
    public string upgradeName;
    [TextArea] public string description;
    public int cost;
    public Sprite icon;
    public float bonusAmount; 
}

[System.Serializable]
public class UpgradePath
{
    public string pathName;
    public UpgradeStat stat;        
    public UpgradeStep[] steps;     
}

[CreateAssetMenu(fileName = "New Upgrade Tree", menuName = "Tower Defense/Upgrade Tree")]
public class TowerUpgradeTree : ScriptableObject
{
    public UpgradePath[] paths = new UpgradePath[3];
}
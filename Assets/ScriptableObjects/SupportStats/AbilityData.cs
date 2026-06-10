using UnityEngine;

public enum AbilityType
{
    Mine,
    CzechHedgehog,
    Mortar,
    Radar
}

[System.Serializable]
public class AbilityUpgradeTier
{
    public string upgradeName;
    [TextArea] public string description;
    public int cost;

    [Header("Stat Modifiers")]
    public float damageBonus;
    public float radiusBonus;
    public float durationBonus;
    public float slowBonus;
    public int maxPlacementsBonus; 
    public int cooldownReduction;
}

[CreateAssetMenu(fileName = "New Ability", menuName = "Tower Defense/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Identity")]
    public AbilityType abilityType;
    public string abilityName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Economy")]
    public int unlockCost;

    [Header("Base Stats")]
    public float baseDamage;
    public float baseRadius;
    public float baseDuration;      
    public float baseSlowAmount;    
    public int baseMaxPlacements;   

    [Header("Cooldown (in rounds)")]
    public int roundCooldown;       

    [Header("Upgrades")]
    public AbilityUpgradeTier[] upgradeTiers; 
}
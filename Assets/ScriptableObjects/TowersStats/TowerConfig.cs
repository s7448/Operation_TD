using UnityEngine;

public abstract class TowerConfig : ScriptableObject
{
    [Header("Handbook UI")]
    [TextArea]
    public string description;

    [Header("Identity")]
    public string towerName;
    public TowerCategory category;
    public Sprite icon;

    [Header("Economy")]
    public int cost = 100;

    [Header("General Stats")]
    public float range = 5f;
    public float fireRate = 1f;

    [Header("Visuals")]
    public AudioClip shootSound; 

    [Header("Upgrade System")]
    public TowerUpgradeTree upgradeTree;
}
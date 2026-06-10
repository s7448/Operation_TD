using UnityEngine;

[CreateAssetMenu(fileName = "New Combat Tower", menuName = "Tower Defense/Combat Tower Config")]
public class CombatTowerConfig : TowerConfig
{
    [Header("Attack Stats")]
    public float softAttack = 10f;
    public float hardAttack = 2f;
    public float piercing = 5f;

    [Header("AOE")]
    public float explosionRadius = 0f;
    public bool useFalloffDamage = false;

    [Header("Slow Effect")]
    public float slowAmount = 0f;    
    public float slowDuration = 0f;

    [Header("Projectile Settings")]
    public float projectileSpeed = 20f;
    public bool isArcing = false;

    [Header("Effects")]
    public GameObject projectilePrefab;
    public GameObject muzzleEffect; 
    public GameObject impactEffect;

    [Header("Default AI")]
    public TargetingMode defaultTargeting = TargetingMode.First;

}
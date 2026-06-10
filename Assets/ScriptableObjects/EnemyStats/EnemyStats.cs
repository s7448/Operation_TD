using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Tower Defense/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [Header("Handbook UI")]
    public Sprite icon;             
    [TextArea]
    public string description;      
    public int firstWaveAppear;     

    [Header("Identity")]
    public string enemyName;        
    public EnemyType type;          
    public MovementType movement;   

    [Header("HoI4 Defense Stats")]
    [Range(0f, 1f)]
    public float hardness;          
    public float armorValue;        
    public float maxHealth = 100;   
    public float movementSpeed = 2f;

    [Header("Economy")]
    public int goldReward = 10;     
    public int damageToPlayer = 1;  

    [Header("Spawn")]
    public float spawnHeightOffset = 0f;
}
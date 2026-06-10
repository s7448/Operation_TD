using UnityEngine;

[CreateAssetMenu(fileName = "New Support Tower", menuName = "Tower Defense/Support Tower Config")]
public class SupportTowerConfig : TowerConfig
{
    [Header("Support Logic")]
    public SupportType supportType;

    [Header("Effect Data")]
    [Tooltip("Amount to Buff (e.g. 1.5 for +50% speed) or Debuff (e.g. 0.5 for 50% slow)")]
    public float multiplier = 1f;

    [Tooltip("Used for temporary effects or pulses")]
    public float effectDuration = 0f;
}
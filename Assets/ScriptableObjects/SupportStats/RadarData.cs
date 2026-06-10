using UnityEngine;

[CreateAssetMenu(fileName = "New Radar Config", menuName = "Tower Defense/Radar Config")]
public class RadarConfig : TowerConfig
{
    [Header("Radar")]
    public float buffRadius = 8f;
}
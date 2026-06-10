using UnityEngine;
using System.Collections.Generic;

public class SupportTower : MonoBehaviour
{
    [Header("Configuration")]
    public SupportTowerConfig config;

    [Header("Visuals")]
    public Transform partToRotate;

    private float tickTimer = 0f;
    private float tickRate = 0.5f; 

    void Update()
    {
        if (partToRotate != null)
        {
            partToRotate.Rotate(Vector3.up * 50f * Time.deltaTime);
        }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            ApplySupportEffect();
            tickTimer = tickRate;
        }
    }

    void ApplySupportEffect()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, config.range);

        foreach (Collider hit in hits)
        {
            if (config.supportType == SupportType.SlowAura)
            {
                EnemyController enemy = hit.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.ApplySpeedModifier(config.multiplier, tickRate + 0.1f);
                }
            }
            else if (config.supportType == SupportType.AttackSpeedBuff || config.supportType == SupportType.DamageBuff)
            {
                if (hit.gameObject == gameObject) continue;

                CombatTower allyTower = hit.GetComponent<CombatTower>();
                if (allyTower != null)
                {
                    allyTower.ApplyBuff(config.supportType, config.multiplier, tickRate + 0.1f);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, config.range);
        }
    }
}
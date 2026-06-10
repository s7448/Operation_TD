using UnityEngine;

public static class DamageCalculator
{
    private const float MIN_DAMAGE_PERCENT = 0.1f; 

    public static float CalculateDamage(
    CombatTowerConfig weapon,
    EnemyStats victim,
    out DamageType damageType,
    float damageBonus = 0f,
    float piercingBonus = 0f,
    float hardAttackBonus = 0f)
    {
        float softDamage = (weapon.softAttack + damageBonus) * (1f - victim.hardness);
        float hardDamage = (weapon.hardAttack + hardAttackBonus) * victim.hardness;
        float totalDamage = softDamage + hardDamage;

        float totalPiercing = weapon.piercing + piercingBonus;

        if (totalPiercing < victim.armorValue)
        {
            float penetrationRatio = totalPiercing / victim.armorValue;
            penetrationRatio = Mathf.Max(penetrationRatio, MIN_DAMAGE_PERCENT);
            totalDamage *= penetrationRatio;
            damageType = DamageType.Blocked;
        }
        else if (totalPiercing > victim.armorValue)
        {
            damageType = DamageType.Piercing;
        }
        else
        {
            damageType = DamageType.Normal;
        }

        return Mathf.Max(0f, totalDamage);
    }
}
using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private CombatTowerConfig stats;
    private Transform target;

    private float dmgMultiplier;
    private float addedDamage;
    private float addedPierce;

    private int piercesRemaining;
    private float damageMultiplierPerPierce = 0.75f;

    private bool isArcing;
    private Vector3 startPos;
    private Vector3 targetPos;
    private float arcHeight;
    private float arcDuration;
    private float arcTimer;
    private bool arcTargetLocked;

    public void Initialize(CombatTowerConfig weaponStats, Transform enemyTarget,
        float buffMult, float flatDamageMod, float flatPierceMod)
    {
        stats = weaponStats;
        target = enemyTarget;
        dmgMultiplier = buffMult;
        addedDamage = flatDamageMod;
        addedPierce = flatPierceMod;

        float totalPiercing = stats.piercing + addedPierce;
        if (totalPiercing > 0 && enemyTarget != null)
        {
            EnemyController enemy = enemyTarget.GetComponent<EnemyController>();
            if (enemy != null && totalPiercing > enemy.stats.armorValue)
            {
                float excess = totalPiercing - enemy.stats.armorValue;
                piercesRemaining = Mathf.FloorToInt(excess / 10f);
            }
        }

        isArcing = stats.isArcing;
        if (isArcing && enemyTarget != null)
        {
            startPos = transform.position;
            targetPos = GetTargetAimPoint();

            float distance = Vector3.Distance(startPos, targetPos);
            arcHeight = distance * 0.5f;
            arcDuration = distance / stats.projectileSpeed;
            arcTimer = 0f;
            arcTargetLocked = false; 
        }

        StopAllCoroutines();
        StartCoroutine(TimeoutReturn());
    }

    private IEnumerator TimeoutReturn()
    {
        yield return new WaitForSeconds(8f);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        StopAllCoroutines();
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }

    public void InitializeArcingFixed(Vector3 fixedTargetPos)
    {
        isArcing = true;
        startPos = transform.position;
        targetPos = fixedTargetPos;
        arcTargetLocked = true;

        float distance = Vector3.Distance(startPos, targetPos);
        arcHeight = distance * 0.5f;
        arcDuration = distance / stats.projectileSpeed;
        arcTimer = 0f;
    }

    public void InitializeArcingTracking()
    {
        isArcing = true;
        arcTargetLocked = false;
        startPos = transform.position;

        if (target != null)
        {
            targetPos = GetTargetAimPoint(); 
            float distance = Vector3.Distance(startPos, targetPos);
            arcHeight = distance * 0.5f;
            arcDuration = distance / stats.projectileSpeed;
        }

        arcTimer = 0f;
    }

    void Update()
    {
        if (isArcing)
        {
            HandleArcMovement();
        }
        else
        {
            HandleDirectMovement();
        }
    }

    // -------------------------------------------------------
    // DIRECT MOVEMENT (standard projectile)

    void HandleDirectMovement()
    {
        if (target == null || !target.gameObject.activeSelf)
        {
            ReturnToPool();
            return;
        }

        Vector3 aimPoint = GetTargetAimPoint();
        Vector3 dir = (aimPoint - transform.position).normalized;
        float step = stats.projectileSpeed * Time.deltaTime;
        transform.position += dir * step;
        transform.LookAt(aimPoint);

        if (Vector3.Distance(transform.position, aimPoint) < 0.3f)
            HitTarget();
    }

    // -------------------------------------------------------
    // ARC MOVEMENT

    void HandleArcMovement()
    {
        arcTimer += Time.deltaTime;
        float t = Mathf.Clamp01(arcTimer / arcDuration);

        if (!arcTargetLocked && target != null)
            targetPos = GetTargetAimPoint();

        Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
        currentPos.y += arcHeight * Mathf.Sin(Mathf.PI * t);

        if (t < 1f)
        {
            Vector3 nextPos = Vector3.Lerp(startPos, targetPos, t + 0.01f);
            nextPos.y += arcHeight * Mathf.Sin(Mathf.PI * (t + 0.01f));
            transform.rotation = Quaternion.LookRotation(nextPos - transform.position);
        }

        transform.position = currentPos;

        if (t >= 1f)
            HitTarget();
    }

    // -------------------------------------------------------
    // HIT

    void HitTarget()
    {
        if (stats.explosionRadius > 0)
            Explode();
        else
            HitSingleTarget();

        if (stats.impactEffect != null)
        {
            GameObject impact = Instantiate(stats.impactEffect, transform.position, Quaternion.identity);
            impact.transform.localScale = Vector3.one * 0.1f;
            Destroy(impact, 2f);
        }

        ReturnToPool();
    }


    private Vector3 GetTargetAimPoint()
    {
        if (target == null) return Vector3.zero;

        Collider col = target.GetComponent<Collider>();
        if (col != null) return col.bounds.center;

        return target.position + Vector3.up * 1f;
    }

    void HitSingleTarget()
    {
        if (target == null || !target.gameObject.activeSelf) return;
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null) DealDamage(enemy, 1f);
    }

    void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.explosionRadius);
        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
            {
                DealDamage(enemy, 1f);

                if (stats.slowAmount > 0)
                    enemy.ApplySpeedModifier(1f - stats.slowAmount, stats.slowDuration);
            }
        }
    }

    void DealDamage(EnemyController enemy, float damageScale)
    {
        DamageType damageType;
        float damage = DamageCalculator.CalculateDamage(
            weapon: stats,
            victim: enemy.stats,
            damageType: out damageType,
            damageBonus: addedDamage,
            piercingBonus: addedPierce
        );

        damage *= dmgMultiplier * damageScale;
        enemy.TakeDamage(damage);

        if (DamageNumberSpawner.Instance != null)
            DamageNumberSpawner.Instance.Spawn(
                enemy.transform.position,
                damage,
                damageType
            );

        if (piercesRemaining > 0 && !isArcing && stats.explosionRadius <= 0)
            PierceThrough(enemy);
    }

    void PierceThrough(EnemyController hitEnemy)
    {
        piercesRemaining--;
        dmgMultiplier *= damageMultiplierPerPierce;

        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            transform.forward,
            5f 
        );

        foreach (RaycastHit hit in hits)
        {
            EnemyController next = hit.collider.GetComponent<EnemyController>();
            if (next != null && next != hitEnemy)
            {
                DealDamage(next, 1f);
                break;
            }
        }
    }

}
using UnityEngine;

[RequireComponent(typeof(Targeting))]
public class CombatTower : MonoBehaviour
{
    [Header("Configuration")]
    public CombatTowerConfig config;

    [Header("Visual Setup")]
    public Transform partToRotate;

    [Tooltip("Assign 1 point for standard turrets, or multiple for MLRS/Javelin")]
    public Transform[] firePoints;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask buildingLayer;

    private int currentFirePointIndex = 0;
    public AudioSource audioSource;

    [Header("Debug / State")]
    [SerializeField] private float fireCooldown = 0f;
    [SerializeField] private Transform currentTarget;

    private float damageMultiplier = 1f;
    private float fireRateMultiplier = 1f;
    private float buffTimer = 0f;

    private float damageMod = 0f;
    private float hardAttackMod = 0f;
    private float rangeMod = 0f;
    private float piercingMod = 0f;
    private float fireRateMod = 0f;

    private float radarRangeBuff = 0f;
    private float radarFireRateBuff = 0f;
    private float radarDamageBuff = 0f;

    [Header("Placement")]
    public float pivotYOffset = 0f;

    private Targeting targetingSystem;

    void Start()
    {
        targetingSystem = GetComponent<Targeting>();
    }

    void Update()
    {
        HandleBuffs();
        HandleCombat();
    }

    void HandleBuffs()
    {
        if (buffTimer > 0)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0)
            {
                damageMultiplier = 1f;
                fireRateMultiplier = 1f;
            }
        }
    }

    void HandleCombat()
    {
        fireCooldown -= Time.deltaTime;

        float effectiveRange = (config.range + rangeMod) * (1f + radarRangeBuff);
        float effectiveFireRate = (config.fireRate + fireRateMod) * (1f + radarFireRateBuff);

        targetingSystem.UpdateTargeting(effectiveRange);
        currentTarget = targetingSystem.GetTargetTransform();

        if (currentTarget != null)
        {
            bool isArcingTower = GetComponent<MLRSController>() != null ||
                                 GetComponent<JavelinController>() != null;

            if (!isArcingTower && !HasLineOfSight(currentTarget)) return;

            Aim();

            if (fireCooldown <= 0f)
            {
                Shoot();
                if (effectiveFireRate <= 0) effectiveFireRate = 0.1f;
                fireCooldown = 1f / effectiveFireRate;
            }
        }
    }

    void Aim()
    {
        if (partToRotate == null) return;

        Vector3 dir = currentTarget.position - transform.position;
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRot, Time.deltaTime * 10f).eulerAngles;
            partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }
    }

    void Shoot()
    {
        if (config.projectilePrefab == null || firePoints.Length == 0) return;

        Transform spawnPoint = firePoints[currentFirePointIndex];

        MLRSController mlrs = GetComponent<MLRSController>();
        if (mlrs != null)
        {
            mlrs.FireBurst(currentTarget);
            return;
        }

        JavelinController javelin = GetComponent<JavelinController>();
        if (javelin != null)
        {
            javelin.FireJavelin(currentTarget, spawnPoint);
            currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
            return;
        }

        GameObject bulletGO = ObjectPool.Instance != null
            ? ObjectPool.Instance.Get(config.projectilePrefab, spawnPoint.position, spawnPoint.rotation)
            : Instantiate(config.projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        Projectile proj = bulletGO.GetComponent<Projectile>();
        if (proj != null)
            proj.Initialize(config, currentTarget, damageMultiplier, damageMod + GetRadarDamageBonus(), piercingMod);

        if (config.muzzleEffect != null)
        {
            GameObject muzzle = Instantiate(config.muzzleEffect, spawnPoint.position, spawnPoint.rotation);
            muzzle.transform.localScale = Vector3.one * 0.1f;
            Destroy(muzzle, 0.1f);
        }

        if (config.shootSound != null && audioSource != null)
            audioSource.PlayOneShot(config.shootSound);

        currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;
    }

    private float GetRadarDamageBonus()
    {
        if (config == null) return 0f;
        float higherAttack = Mathf.Max(config.softAttack, config.hardAttack);
        return higherAttack * radarDamageBuff;
    }
    public void ModifyStats(float softAtk, float fireRate, float range, float piercing, float hardAtk = 0f)
    {
        damageMod += softAtk;
        hardAttackMod += hardAtk;
        rangeMod += range;
        piercingMod += piercing;
        fireRateMod += fireRate; 
    }


    public void ApplyBuff(SupportType type, float value, float duration)
    {
        buffTimer = duration;

        if (type == SupportType.AttackSpeedBuff)
        {
            fireRateMultiplier = value;
        }
        else if (type == SupportType.DamageBuff)
        {
            damageMultiplier = value;
        }
    }

    public void SetRadarBuff(float rangePct, float fireRatePct, float damagePct)
    {
        radarRangeBuff = rangePct;
        radarFireRateBuff = fireRatePct;
        radarDamageBuff = damagePct;
    }

    public float GetEffectiveRange()
    {
        return (config.range + rangeMod) * (1f + radarRangeBuff);
    }

    private bool HasLineOfSight(Transform target)
    {
        if (firePoints.Length == 0) return true;

        Transform firePoint = firePoints[currentFirePointIndex];
        Vector3 direction = target.position - firePoint.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(firePoint.position, direction.normalized, distance, buildingLayer))
            return false;

        return true;
    }
    void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, GetEffectiveRange()); 
        }
    }
}
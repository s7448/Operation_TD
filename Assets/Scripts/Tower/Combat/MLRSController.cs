using UnityEngine;
using System.Collections;

public class MLRSController : MonoBehaviour
{
    [Header("MLRS Settings")]
    [SerializeField] private float burstInterval = 0.15f;  
    [SerializeField] private float reloadTime = 8f;         
    [SerializeField] private float scatterRadius = 3f;      

    private CombatTower combatTower;
    private bool isFiring = false;
    private bool isReloading = false;

    void Start()
    {
        combatTower = GetComponent<CombatTower>();
    }

    public void FireBurst(Transform target)
    {
        if (isFiring || isReloading) return;
        StartCoroutine(BurstRoutine(target));
    }

    public bool IsReady() => !isFiring && !isReloading;

    IEnumerator BurstRoutine(Transform target)
    {
        isFiring = true;

        Transform[] firePoints = combatTower.firePoints;

        foreach (Transform firePoint in firePoints)
        {
            if (target == null) break;

            EnemyController enemy = target.GetComponent<EnemyController>();
            if (enemy == null || enemy.currentHealth <= 0) break;

            Vector2 scatter = Random.insideUnitCircle * scatterRadius;
            Vector3 impactPoint = target.position + new Vector3(scatter.x, 0f, scatter.y);

            GameObject rocketGO = Instantiate(
                combatTower.config.projectilePrefab,
                firePoint.position,
                firePoint.rotation
            );

            Projectile proj = rocketGO.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(combatTower.config, target, 1f, 0f, 0f);
                proj.InitializeArcingFixed(impactPoint);
            }

            if (combatTower.config.muzzleEffect != null)
                Instantiate(combatTower.config.muzzleEffect, firePoint.position, firePoint.rotation);

            yield return new WaitForSeconds(burstInterval);
        }

        isFiring = false;
        isReloading = true;

        float effectiveReload = reloadTime / combatTower.config.fireRate;
        effectiveReload = Mathf.Max(2f, effectiveReload); 

        yield return new WaitForSeconds(effectiveReload);

        isReloading = false;
    }
}
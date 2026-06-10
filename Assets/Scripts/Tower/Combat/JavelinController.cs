using UnityEngine;

public class JavelinController : MonoBehaviour
{


    private CombatTower combatTower;

    void Start()
    {
        combatTower = GetComponent<CombatTower>();
    }

    public void FireJavelin(Transform target, Transform firePoint)
    {
        if (combatTower.config.projectilePrefab == null) return;

        GameObject missileGO = Instantiate(
            combatTower.config.projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Projectile proj = missileGO.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(combatTower.config, target, 1f, 0f, 0f);
            proj.InitializeArcingTracking();
        }

        if (combatTower.config.muzzleEffect != null)
            Instantiate(combatTower.config.muzzleEffect, firePoint.position, firePoint.rotation);
    }
}
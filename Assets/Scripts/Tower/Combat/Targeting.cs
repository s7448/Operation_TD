using System.Collections.Generic;
using UnityEngine;

public class Targeting : MonoBehaviour
{
    [Header("Settings")]
    public TargetingMode targetingMode = TargetingMode.First;
    public LayerMask enemyLayer;

    [Header("Debug")]
    [SerializeField] private EnemyController currentTarget;
    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    public void UpdateTargeting(float range)
    {
        FindEnemiesInRange(range);
        currentTarget = SelectTarget();
    }

    void FindEnemiesInRange(float range)
    {
        enemiesInRange.Clear();

        Collider[] colliders = Physics.OverlapSphere(transform.position, range, enemyLayer);

        foreach (Collider col in colliders)
        {
            EnemyController enemy = col.GetComponent<EnemyController>();

            if (enemy != null && enemy.currentHealth > 0)
            {
                enemiesInRange.Add(enemy);
            }
        }
    }

    EnemyController SelectTarget()
    {
        if (enemiesInRange.Count == 0) return null;

        switch (targetingMode)
        {
            case TargetingMode.First:
                return GetFirstEnemy();
            case TargetingMode.Last:
                return GetLastEnemy();
            case TargetingMode.Strongest:
                return GetStrongestEnemy();
            case TargetingMode.Weakest:
                return GetWeakestEnemy();
            case TargetingMode.Closest:
                return GetClosestEnemy();
            default:
                return GetFirstEnemy();
        }
    }
    EnemyController GetFirstEnemy()
    {
        EnemyController best = null;
        float maxProgress = -1f;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;
            if (e.GetPathProgress() > maxProgress)
            {
                maxProgress = e.GetPathProgress();
                best = e;
            }
        }
        return best;
    }

    EnemyController GetLastEnemy()
    {
        EnemyController best = null;
        float minProgress = float.MaxValue;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;
            if (e.GetPathProgress() < minProgress)
            {
                minProgress = e.GetPathProgress();
                best = e;
            }
        }
        return best;
    }

    EnemyController GetStrongestEnemy()
    {
        EnemyController best = null;
        float maxHealth = -1f;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;

            if (e.currentHealth > maxHealth)
            {
                maxHealth = e.currentHealth;
                best = e;
            }
        }
        return best;
    }

    EnemyController GetWeakestEnemy()
    {
        EnemyController best = null;
        float minHealth = float.MaxValue;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;

            if (e.currentHealth < minHealth)
            {
                minHealth = e.currentHealth;
                best = e;
            }
        }
        return best;
    }

    EnemyController GetClosestEnemy()
    {
        EnemyController best = null;
        float closestDist = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var e in enemiesInRange)
        {
            if (e == null) continue;
            float dist = Vector3.SqrMagnitude(e.transform.position - myPos);
            if (dist < closestDist)
            {
                closestDist = dist;
                best = e;
            }
        }
        return best;
    }


    public Transform GetTargetTransform()
    {
        if (currentTarget != null) return currentTarget.transform;
        return null;
    }

    public EnemyController GetTargetEnemy()
    {
        return currentTarget;
    }

    public void CycleTargetingMode()
    {
        int current = (int)targetingMode;
        int next = (current + 1) % System.Enum.GetValues(typeof(TargetingMode)).Length;
        targetingMode = (TargetingMode)next;
    }
}
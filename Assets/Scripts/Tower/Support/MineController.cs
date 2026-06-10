using UnityEngine;

public class MineController : MonoBehaviour
{
    private float damage;
    private float radius;
    private bool hasExploded = false;

    [Header("Effects")]
    [SerializeField] private GameObject explosionEffect;

    public void Initialize()
    {
        if (AbilityManager.Instance == null) return;

        AbilityData data = AbilityManager.Instance.GetData(AbilityType.Mine);
        int level = AbilityManager.Instance.GetUpgradeLevel(AbilityType.Mine);

        damage = data.baseDamage;
        radius = data.baseRadius;


        for (int i = 0; i < level; i++)
        {
            damage += data.upgradeTiers[i].damageBonus;
            radius += data.upgradeTiers[i].radiusBonus;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null) return;

        Explode();
    }

    private void Explode()
    {
        hasExploded = true;

        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
            effect.transform.localScale = Vector3.one * radius * 0.1f;
            Destroy(effect, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.RegisterMineExploded(gameObject);

        Destroy(gameObject);
    }

    public void RemoveMine()
    {
        if (AbilityManager.Instance != null)
            AbilityManager.Instance.RegisterMineRemoved(gameObject);

        Destroy(gameObject);
    }
}
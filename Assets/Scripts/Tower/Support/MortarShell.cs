using UnityEngine;

public class MortarShell : MonoBehaviour
{
    private Vector3 targetPosition;
    private float damage;
    private float radius;
    private float fallSpeed = 30f;

    [SerializeField] private GameObject explosionEffect;

    public void Initialize(Vector3 target, float dmg, float rad)
    {
        targetPosition = target;
        damage = dmg;
        radius = rad;
    }

    void Update()
    {
        Vector3 dir = (targetPosition - transform.position).normalized;
        transform.position += dir * fallSpeed * Time.deltaTime;
        transform.LookAt(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            Explode();
    }

    private void Explode()
    {
        if (explosionEffect != null)
        {
            GameObject effect = Instantiate(
                explosionEffect,
                transform.position,
                Quaternion.identity
            );
            effect.transform.localScale = Vector3.one * radius * 0.1f;
            Destroy(effect, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
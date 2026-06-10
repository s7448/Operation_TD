using UnityEngine;

public class HedgehogController : MonoBehaviour
{
    private float slowAmount;
    private float duration;
    private float timer;

    public void Initialize(float slow, float dur)
    {
        slowAmount = slow;
        duration = dur;
        timer = dur;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy != null)
            enemy.ApplySpeedModifier(1f - slowAmount, 0.5f);
    }
}
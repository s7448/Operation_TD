using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance { get; private set; }

    [SerializeField] private GameObject damageNumberPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Spawn(Vector3 position, float damage, DamageType type)
    {
        if (damageNumberPrefab == null) return;

        Vector3 spawnPos = position + Vector3.up * 1f;

        GameObject go = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
        DamageNumber dn = go.GetComponent<DamageNumber>();
        if (dn != null)
            dn.Initialize(damage, type);
    }
}
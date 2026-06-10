using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyStats stats;

    [Header("Current State")]
    public float currentHealth;
    public float currentSpeed;
    private bool isDead = false;

    private float speedModifier = 1f;
    private float modifierTimer = 0f;

    [Header("Pathfinding")]
    private Transform[] pathNodes;
    private int nodeIndex = 0;
    private Transform currentTargetNode;

    [Header("UI")]
    [SerializeField] private EnemyHealthBar healthBar;

    public static event Action<EnemyController> OnEnemyDeath;
    public static event Action<int> OnLivesLost;
    public static event Action<EnemyController> OnEnemyEscaped;
    public event Action OnDeath;

    void Start()
    {
        InitializeStats();
        InitializePath();

        if (healthBar != null)
            healthBar.UpdateHealth(currentHealth, stats.maxHealth);
    }

    void Update()
    {
        HandleMovement();
        HandleStatusEffects();
    }

    void InitializeStats()
    {
        if (stats == null)
        {
            Debug.LogError($"Enemy {name} is missing EnemyStats!");
            return;
        }

        currentHealth = stats.maxHealth;
        currentSpeed = stats.movementSpeed;
    }

    public void SetPath(Transform[] nodes)
    {
        pathNodes = nodes;
        nodeIndex = 0;
        if (pathNodes.Length > 0)
            currentTargetNode = pathNodes[0];
    }

    public void Prepare(Transform[] nodes)
    {
        isDead = false;
        OnDeath = null; 
        currentHealth = stats.maxHealth;
        currentSpeed = stats.movementSpeed;
        speedModifier = 1f;
        modifierTimer = 0f;
        SetPath(nodes);
        if (healthBar != null)
            healthBar.UpdateHealth(currentHealth, stats.maxHealth);
    }

    void InitializePath()
    {
        if (pathNodes != null && pathNodes.Length > 0) return;

        GameObject nodeParent = GameObject.Find("NODES");
        if (nodeParent != null)
        {
            pathNodes = new Transform[nodeParent.transform.childCount];
            for (int i = 0; i < pathNodes.Length; i++)
                pathNodes[i] = nodeParent.transform.GetChild(i);

            if (pathNodes.Length > 0)
                currentTargetNode = pathNodes[0];
        }
    }

    void HandleMovement()
    {
        if (currentTargetNode == null) return;

        float actualSpeed = stats.movementSpeed * speedModifier;

        Vector3 targetXZ = new Vector3(currentTargetNode.position.x, transform.position.y, currentTargetNode.position.z);
        Vector3 direction = targetXZ - transform.position;
        float distThisFrame = actualSpeed * Time.deltaTime;

        if (direction.magnitude <= distThisFrame)
        {
            GetNextNode();
        }
        else
        {
            transform.Translate(direction.normalized * distThisFrame, Space.World);

            if (direction != Vector3.zero)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }
        }
    }

    void GetNextNode()
    {
        nodeIndex++;
        if (nodeIndex >= pathNodes.Length)
        {
            ReachEndOfPath();
            return;
        }
        currentTargetNode = pathNodes[nodeIndex];
    }

    public void TakeDamage(float amount)
    {
        if (isDead || !gameObject.activeSelf) return;

        currentHealth -= amount;

        if (healthBar != null)
            healthBar.UpdateHealth(currentHealth, stats.maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void ApplySpeedModifier(float multiplier, float duration)
    {
        if (multiplier < speedModifier)
            speedModifier = multiplier;

        modifierTimer = Mathf.Max(modifierTimer, duration);
    }

    void HandleStatusEffects()
    {
        if (modifierTimer > 0)
        {
            modifierTimer -= Time.deltaTime;
            if (modifierTimer <= 0)
                speedModifier = 1f;
        }
    }

    public float GetPathProgress()
    {
        if (pathNodes == null || pathNodes.Length == 0) return 0f;
        if (nodeIndex >= pathNodes.Length) return 1f;

        float baseProgress = (float)nodeIndex / pathNodes.Length;

        if (currentTargetNode != null && nodeIndex < pathNodes.Length)
        {
            Vector3 startPos = (nodeIndex > 0) ? pathNodes[nodeIndex - 1].position : transform.position;
            float segmentTotal = Vector3.Distance(startPos, currentTargetNode.position);
            float distToTarget = Vector3.Distance(transform.position, currentTargetNode.position);

            if (segmentTotal > 0)
            {
                float segmentPercent = (segmentTotal - distToTarget) / segmentTotal;
                baseProgress += segmentPercent / pathNodes.Length;
            }
        }

        return Mathf.Clamp01(baseProgress);
    }

    void ReachEndOfPath()
    {
        OnLivesLost?.Invoke(stats.damageToPlayer);
        OnEnemyEscaped?.Invoke(this);
        ReturnToPool();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke();
        OnEnemyDeath?.Invoke(this);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null)
            ObjectPool.Instance.Return(gameObject);
        else
            Destroy(gameObject);
    }
}
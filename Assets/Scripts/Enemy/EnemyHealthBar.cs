using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Canvas canvas;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (canvas != null && mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthFillImage != null)
        {
            float pct = Mathf.Clamp01(currentHealth / maxHealth);
            healthFillImage.fillAmount = pct;

            healthFillImage.color = Color.Lerp(Color.red, Color.green, pct);
        }
    }
}
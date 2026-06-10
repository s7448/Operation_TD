using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro text;
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;

    private float timer;
    private Color currentColor;

    void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        text.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    public void Initialize(float damage, DamageType damageType)
    {
        text.text = Mathf.RoundToInt(damage).ToString();

        switch (damageType)
        {
            case DamageType.Normal:
                currentColor = Color.white;
                break;
            case DamageType.Piercing:
                currentColor = Color.red;
                break;
            case DamageType.Blocked:
                currentColor = Color.gray;
                break;
        }

        text.color = currentColor;

        Camera cam = Camera.main;
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }

    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }
}
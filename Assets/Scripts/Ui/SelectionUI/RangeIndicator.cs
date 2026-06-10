using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private int segments = 64;
    [SerializeField] private GameObject fillObject;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.05f;
    }

    public void Show(float radius)
    {
        gameObject.SetActive(true);
        DrawCircle(radius);

        if (fillObject != null)
        {
            fillObject.transform.position = new Vector3(
                transform.position.x,
                transform.position.y + 0.1f,
                transform.position.z
            );
            fillObject.transform.localScale = new Vector3(radius * 2f, 0.01f, radius * 2f);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void DrawCircle(float radius)
    {
        lineRenderer.positionCount = segments + 1;
        Vector3 center = transform.position;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(
                center.x + x,
                center.y + 0.15f,
                center.z + z
            ));
        }
    }

}
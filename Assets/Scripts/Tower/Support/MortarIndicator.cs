using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MortarIndicator : MonoBehaviour
{
    [SerializeField] private int segments = 40;
    [SerializeField] private float radius = 1f;
    [SerializeField] private LineRenderer crosshairH; 
    [SerializeField] private LineRenderer crosshairV; 

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        DrawCircle(radius);
        DrawCrosshair();
    }

    public void SetRadius(float newRadius)
    {
        radius = newRadius;
        DrawCircle(radius);
        DrawCrosshair();
    }

    private void DrawCircle(float r)
    {
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float x = Mathf.Cos(angle) * r;
            float z = Mathf.Sin(angle) * r;
            lineRenderer.SetPosition(i, new Vector3(x, 0.05f, z));
        }
    }

    private void DrawCrosshair()
    {
        float size = radius * 0.3f; 

        if (crosshairH != null)
        {
            crosshairH.positionCount = 2;
            crosshairH.SetPosition(0, new Vector3(-size, 0.05f, 0));
            crosshairH.SetPosition(1, new Vector3(size, 0.05f, 0));
        }

        if (crosshairV != null)
        {
            crosshairV.positionCount = 2;
            crosshairV.SetPosition(0, new Vector3(0, 0.05f, -size));
            crosshairV.SetPosition(1, new Vector3(0, 0.05f, size));
        }
    }
}
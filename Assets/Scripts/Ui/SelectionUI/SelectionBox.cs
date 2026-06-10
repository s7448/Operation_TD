using UnityEngine;

public class SelectionBox : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private Material boxMaterial;

    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.loop = false;
        if (boxMaterial != null)
            lineRenderer.material = boxMaterial;
    }

    public void Show(Bounds bounds)
    {
        gameObject.SetActive(true);
        DrawBox(bounds);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void DrawBox(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        float yTop = max.y + 0.1f;
        float yBot = min.y;

        Vector3 p0 = new Vector3(min.x, yBot, min.z);
        Vector3 p1 = new Vector3(max.x, yBot, min.z);
        Vector3 p2 = new Vector3(max.x, yBot, max.z);
        Vector3 p3 = new Vector3(min.x, yBot, max.z);
        Vector3 p4 = new Vector3(min.x, yTop, min.z);
        Vector3 p5 = new Vector3(max.x, yTop, min.z);
        Vector3 p6 = new Vector3(max.x, yTop, max.z);
        Vector3 p7 = new Vector3(min.x, yTop, max.z);

        Vector3[] points = new Vector3[]
        {
        // bottom rectangle
        p0, p1, p2, p3, p0,
        // up to top
        p4,
        // top rectangle
        p5, p6, p7, p4,
        // vertical edges
        p7, p3,
        p2, p6,
        p5, p1,
        };

        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
}
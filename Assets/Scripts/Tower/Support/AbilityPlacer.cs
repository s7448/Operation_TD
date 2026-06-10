using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityPlacer : MonoBehaviour
{
    public static AbilityPlacer Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RectTransform gameAreaRect;

    [Header("Prefabs")]
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private GameObject hedgehogPrefab;
    [SerializeField] private GameObject mortarIndicatorPrefab; 

    [Header("Layers")]
    [SerializeField] private LayerMask pathLayer;    
    [SerializeField] private LayerMask groundLayer;  

    [Header("Mortar")]
    [SerializeField] private GameObject mortarShellPrefab;

    [Header("UI")]
    [SerializeField] private RectTransform upgradePanelRect;
    [SerializeField] private RectTransform supportPanelRect;

    [Header("Placement")]
    [SerializeField] private float previewYOffset = 0;


    private enum PlacementMode { None, Mine, Hedgehog, Mortar }
    private PlacementMode currentMode = PlacementMode.None;

    private GameObject currentIndicator; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Update()
    {
        if (currentMode == PlacementMode.None) return;

        UpdateIndicator();
        HandleInput();
    }

    // -------------------------------------------------------
    // START PLACEMENT

    public void StartPlacingMine()
    {
        Debug.Log($"StartPlacingMine called — CanPlace: {AbilityManager.Instance.CanPlaceMine()}");
        if (!AbilityManager.Instance.CanPlaceMine()) return;
        CancelPlacement();
        currentMode = PlacementMode.Mine;
        currentIndicator = Instantiate(minePrefab);
        DisableComponents(currentIndicator);
    }

    public void StartPlacingHedgehog()
    {
        if (!AbilityManager.Instance.IsReady(AbilityType.CzechHedgehog)) return;
        CancelPlacement();
        currentMode = PlacementMode.Hedgehog;
        currentIndicator = Instantiate(hedgehogPrefab);
        DisableComponents(currentIndicator);
    }

    public void StartMortarAim()
    {
        if (!AbilityManager.Instance.IsReady(AbilityType.Mortar)) return;
        CancelPlacement();
        currentMode = PlacementMode.Mortar;

        if (mortarIndicatorPrefab != null)
        {
            currentIndicator = Instantiate(mortarIndicatorPrefab);
            MortarIndicator indicator = currentIndicator.GetComponent<MortarIndicator>();
            if (indicator != null)
                indicator.SetRadius(AbilityManager.Instance.GetMortarRadius());
        }
    }

    // -------------------------------------------------------
    // UPDATE

    private void UpdateIndicator()
    {
        if (currentIndicator == null) return;

        Ray ray = GetGameAreaRay();
        RaycastHit hit;

        LayerMask targetLayer = currentMode == PlacementMode.Mortar ? groundLayer : pathLayer;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 previewPos = hit.point;
            previewPos.y += previewYOffset;
            currentIndicator.transform.position = previewPos;
            currentIndicator.SetActive(true);
        }
        else
        {
            currentIndicator.SetActive(false);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
            return;
        }

        if (Input.GetMouseButtonDown(0) && IsMouseOverGameArea() && !IsMouseOverAnyPanel())
        {
            Ray ray = GetGameAreaRay();
            RaycastHit hit;

            LayerMask targetLayer = currentMode == PlacementMode.Mortar ? groundLayer : pathLayer;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
            {
                switch (currentMode)
                {
                    case PlacementMode.Mine:
                        PlaceMine(hit.point);
                        break;
                    case PlacementMode.Hedgehog:
                        PlaceHedgehog(hit.point);
                        break;
                    case PlacementMode.Mortar:
                        FireMortar(hit.point);
                        break;
                }
            }
        }
    }

    // -------------------------------------------------------
    // PLACE

    private void PlaceMine(Vector3 position)
    {
        if (!AbilityManager.Instance.CanPlaceMine()) return;

        GameObject mine = Instantiate(minePrefab, position, Quaternion.identity);

        Renderer[] renderers = mine.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
                bounds.Encapsulate(r.bounds);

            float bottomOffset = mine.transform.position.y - bounds.min.y;
            mine.transform.position = new Vector3(
                position.x,
                position.y + bottomOffset,
                position.z
            );
        }

        MineController mineCtrl = mine.GetComponent<MineController>();
        if (mineCtrl != null) mineCtrl.Initialize();

        AbilityManager.Instance.RegisterMinePlaced(mine);

        if (!AbilityManager.Instance.CanPlaceMine())
            CancelPlacement();
    }

    private void PlaceHedgehog(Vector3 position)
    {
        Vector3 spawnPos = new Vector3(position.x, position.y + 0.05f, position.z);
        GameObject hedgehog = Instantiate(hedgehogPrefab, spawnPos, Quaternion.identity);

        HedgehogController hCtrl = hedgehog.GetComponent<HedgehogController>();
        if (hCtrl != null)
        {
            hCtrl.Initialize(
                AbilityManager.Instance.GetHedgehogSlow(),
                AbilityManager.Instance.GetHedgehogDuration()
            );
        }

        AbilityManager.Instance.UseHedgehog();
        CancelPlacement();
    }

    private void FireMortar(Vector3 targetPosition)
    {
        if (mortarShellPrefab != null)
        {
            GameObject shell = Instantiate(
                mortarShellPrefab,
                targetPosition + Vector3.up * 30f, 
                Quaternion.identity
            );

            MortarShell mortarShell = shell.GetComponent<MortarShell>();
            if (mortarShell != null)
            {
                mortarShell.Initialize(
                    targetPosition,
                    AbilityManager.Instance.GetMortarDamage(),
                    AbilityManager.Instance.GetMortarRadius()
                );
            }
        }

        AbilityManager.Instance.UseMortar();
        CancelPlacement();
    }

    // -------------------------------------------------------
    // CANCEL

    public void CancelPlacement()
    {
        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
            currentIndicator = null;
        }
        currentMode = PlacementMode.None;
    }

    public bool IsInPlacementMode() => currentMode != PlacementMode.None;

    // -------------------------------------------------------
    // HELPERS

    private void DisableComponents(GameObject obj)
    {
        foreach (MonoBehaviour script in obj.GetComponentsInChildren<MonoBehaviour>())
            script.enabled = false;
        foreach (Collider col in obj.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    private bool IsMouseOverGameArea()
    {
        Vector3[] corners = new Vector3[4];
        gameAreaRect.GetWorldCorners(corners);
        Vector2 mousePos = Input.mousePosition;
        return mousePos.x >= corners[0].x && mousePos.x <= corners[2].x &&
               mousePos.y >= corners[0].y && mousePos.y <= corners[2].y;
    }

    private bool IsMouseOverAnyPanel()
    {
        return IsMouseOverRect(upgradePanelRect) || IsMouseOverRect(supportPanelRect);
    }

    private bool IsMouseOverRect(RectTransform rect)
    {
        if (rect == null) return false;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 mousePos = Input.mousePosition;
        return mousePos.x >= corners[0].x && mousePos.x <= corners[2].x &&
               mousePos.y >= corners[0].y && mousePos.y <= corners[2].y;
    }

    private Ray GetGameAreaRay()
    {
        Vector3[] corners = new Vector3[4];
        gameAreaRect.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];
        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;
        Vector2 mousePos = Input.mousePosition;
        float u = (mousePos.x - bottomLeft.x) / width;
        float v = (mousePos.y - bottomLeft.y) / height;
        return gameCamera.ViewportPointToRay(new Vector3(u, v, 0));
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RectTransform gameAreaRect;

    [Header("Placement Settings")]
    [SerializeField] private LayerMask placementLayer;
    [SerializeField] private LayerMask blockedLayer;
    [SerializeField] private LayerMask buildingPlacementLayer;
    [SerializeField] private LayerMask buildingLayer;
    [SerializeField] private float gridSize = 2f;
    [SerializeField] private Vector3 gridOffset;

    [Header("Visual Feedback")]
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;

    [Header("Placement Rules")]
    [SerializeField] private float minDistanceBetweenTowers = 2f;
    [SerializeField] private LayerMask towerLayer;

    private GameObject currentTowerPreview;
    private GameObject selectedTowerPrefab;
    private int towerCost;
    private bool isPlacementMode = false;
    private bool canPlaceAtCurrentPosition = false;
    private bool placementJustStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Update()
    {
        if (isPlacementMode) HandlePlacementMode();
    }

    public void StartPlacingTower(GameObject towerPrefab, int cost)
    {
        if (isPlacementMode) CancelPlacement();

        if (GameManager.Instance != null && !GameManager.Instance.CanAfford(cost))
        {
            Debug.Log("Not enough money!");
            return;
        }

        selectedTowerPrefab = towerPrefab;
        towerCost = cost;
        isPlacementMode = true;
        placementJustStarted = true;

        CreatePreview();
    }

    private void CreatePreview()
    {
        currentTowerPreview = Instantiate(selectedTowerPrefab);
        DisableTowerComponents(currentTowerPreview);
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

    private bool IsMouseOverGameArea()
    {
        Vector3[] corners = new Vector3[4];
        gameAreaRect.GetWorldCorners(corners);

        Vector2 mousePos = Input.mousePosition;

        return mousePos.x >= corners[0].x && mousePos.x <= corners[2].x &&
               mousePos.y >= corners[0].y && mousePos.y <= corners[2].y;
    }

    private void HandlePlacementMode()
    {
        if (placementJustStarted)
        {
            placementJustStarted = false;
            return;
        }

        Ray ray = GetGameAreaRay();
        LayerMask combinedLayers = placementLayer | buildingPlacementLayer | buildingLayer;
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity, combinedLayers);

        if (hits.Length > 0)
        {
            RaycastHit hit = hits[0];
            bool hitRooftop = false;
            bool hitBuilding = false;
            RaycastHit rooftopHit = default;

            foreach (RaycastHit h in hits)
            {
                int layer = h.collider.gameObject.layer;
                if (((1 << layer) & buildingPlacementLayer) != 0)
                {
                    hitRooftop = true;
                    rooftopHit = h; 
                }
                if (((1 << layer) & buildingLayer) != 0)
                    hitBuilding = true;
            }
            if (hitRooftop)
                hit = rooftopHit;
            else
            {
                foreach (RaycastHit h in hits)
                    if (h.point.y > hit.point.y)
                        hit = h;
            }
            foreach (RaycastHit h in hits)
            {
                int layer = h.collider.gameObject.layer;
                bool isRooftop = ((1 << layer) & buildingPlacementLayer) != 0;
                bool isBuilding = ((1 << layer) & buildingLayer) != 0;
                Debug.Log($"Hit: {h.collider.gameObject.name}, Layer: {layer}, isRooftop: {isRooftop}, isBuilding: {isBuilding}, Y: {h.point.y}");
            }
            Debug.Log($"hitRooftop: {hitRooftop}, hitBuilding: {hitBuilding}");

            Vector3 targetPosition = hit.point;

            if (gridSize > 0)
                targetPosition = SnapToGrid(targetPosition);

            canPlaceAtCurrentPosition = IsValidPlacement(targetPosition, hitRooftop, hitBuilding);
            UpdatePreviewColor(canPlaceAtCurrentPosition);

            CombatTower combatTower = currentTowerPreview.GetComponent<CombatTower>();
            float yOffset = combatTower != null ? combatTower.pivotYOffset : 0f;
            currentTowerPreview.transform.position = targetPosition + new Vector3(0, yOffset, 0);

            if (Input.GetMouseButtonDown(0) && IsMouseOverGameArea())
                TryPlaceTower(targetPosition);
        }
        else
        {
            canPlaceAtCurrentPosition = false;
            UpdatePreviewColor(false);
        }

        if ((Input.GetMouseButtonDown(1) && IsMouseOverGameArea()) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        if (gridSize <= 0) return position;

        Vector3 offsetPosition = position - gridOffset;
        float x = Mathf.Round(offsetPosition.x / gridSize) * gridSize;
        float z = Mathf.Round(offsetPosition.z / gridSize) * gridSize;

        return new Vector3(x + gridOffset.x, position.y, z + gridOffset.z);
    }

    private bool IsValidPlacement(Vector3 position, bool onRooftop = false, bool onBuilding = false)
    {
        if (GameManager.Instance != null && !GameManager.Instance.CanAfford(towerCost))
        {
            Debug.Log("BLOCKED: cant afford");
            return false;
        }

        if (blockedLayer != 0 && Physics.CheckSphere(position, 0.4f, blockedLayer))
        {
            Debug.Log("BLOCKED: blocked layer");
            return false;
        }

        if (Physics.CheckSphere(position, minDistanceBetweenTowers, towerLayer))
        {
            Debug.Log("BLOCKED: too close to tower");
            return false;
        }

        bool onGround = Physics.CheckSphere(position, 0.5f, placementLayer);
        Debug.Log($"onGround: {onGround}, onRooftop: {onRooftop}, onBuilding: {onBuilding}");

        if (onBuilding && !onRooftop)
        {
            Debug.Log("BLOCKED: on building no rooftop");
            return false;
        }

        if (!onGround && !onRooftop)
        {
            Debug.Log("BLOCKED: not on valid surface");
            return false;
        }

        if (onRooftop && !onGround)
        {
            bool isArcingTower = selectedTowerPrefab.GetComponent<MLRSController>() != null ||
                                 selectedTowerPrefab.GetComponent<JavelinController>() != null;
            Debug.Log($"isArcingTower: {isArcingTower}");
            if (!isArcingTower) return false;
        }

        Debug.Log("VALID placement");
        return true;
    }

    private void TryPlaceTower(Vector3 position)
    {
        if (!canPlaceAtCurrentPosition) return;

        if (GameManager.Instance != null)
            if (!GameManager.Instance.SpendMoney(towerCost)) return;

        CombatTower combatTower = currentTowerPreview.GetComponent<CombatTower>();
        float yOffset = combatTower != null ? combatTower.pivotYOffset : 0f;
        Vector3 spawnPosition = new Vector3(position.x, position.y + yOffset, position.z);

        GameObject newTower = Instantiate(selectedTowerPrefab, spawnPosition, Quaternion.identity);
        SetLayerRecursively(newTower, GetLayerFromMask(towerLayer));
    }

    private void UpdatePreviewColor(bool isValid)
    {
        if (currentTowerPreview == null) return;

        Material materialToUse = isValid ? validPlacementMaterial : invalidPlacementMaterial;
        Renderer[] renderers = currentTowerPreview.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
            if (r.sharedMaterial != materialToUse)
                r.material = materialToUse;
    }

    public void CancelPlacement()
    {
        if (currentTowerPreview != null) Destroy(currentTowerPreview);
        isPlacementMode = false;
        selectedTowerPrefab = null;
        placementJustStarted = false;
    }

    public bool IsInPlacementMode() => isPlacementMode;

    private void DisableTowerComponents(GameObject tower)
    {
        foreach (MonoBehaviour script in tower.GetComponentsInChildren<MonoBehaviour>())
            script.enabled = false;

        foreach (Collider col in tower.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    int GetLayerFromMask(LayerMask mask)
    {
        int layerNumber = 0;
        int layer = mask.value;
        while (layer > 0)
        {
            layer = layer >> 1;
            layerNumber++;
        }
        return layerNumber - 1;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (gridSize <= 0.1f) return;
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        float size = 20f;

        for (float x = center.x - size; x <= center.x + size; x += gridSize)
            Gizmos.DrawLine(new Vector3(x, 0, center.z - size), new Vector3(x, 0, center.z + size));

        for (float z = center.z - size; z <= center.z + size; z += gridSize)
            Gizmos.DrawLine(new Vector3(center.x - size, 0, z), new Vector3(center.x + size, 0, z));
    }

    public void SetTowerLayer(GameObject tower)
    {
        SetLayerRecursively(tower, GetLayerFromMask(towerLayer));
    }
}
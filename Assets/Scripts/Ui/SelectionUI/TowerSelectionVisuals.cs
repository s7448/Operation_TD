using UnityEngine;

public class TowerSelectionVisuals : MonoBehaviour
{
    public static TowerSelectionVisuals Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject rangeIndicatorPrefab;
    [SerializeField] private GameObject selectionBoxPrefab;

    private GameObject currentRangeIndicator;
    private GameObject currentSelectionBox;
    private CombatTower currentTower;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.OnTowerSelected += OnTowerSelected;
            InteractionManager.Instance.OnTowerDeselected += OnTowerDeselected;
            InteractionManager.Instance.OnRadarSelected += OnRadarSelected;
            InteractionManager.Instance.OnRadarDeselected += OnRadarDeselected;
        }
    }

    void OnDestroy()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.OnTowerSelected -= OnTowerSelected;
            InteractionManager.Instance.OnTowerDeselected -= OnTowerDeselected;
        }
    }

    void Update()
    {
        if (currentTower != null && currentRangeIndicator != null)
        {
            float effectiveRange = currentTower.GetEffectiveRange();
            
            currentRangeIndicator.GetComponent<RangeIndicator>().Show(effectiveRange);
        }
    }

    private void OnTowerSelected(CombatTower tower)
    {
        currentTower = tower;
        ShowVisuals(tower);
    }

    private void OnTowerDeselected()
    {
        currentTower = null;
        HideVisuals();
    }

    private void ShowVisuals(CombatTower tower)
    {
        HideVisuals();

        if (rangeIndicatorPrefab != null)
        {
            currentRangeIndicator = Instantiate(rangeIndicatorPrefab);
            currentRangeIndicator.transform.position = new Vector3(
                tower.transform.position.x,
                tower.transform.position.y,
                tower.transform.position.z
            );
            currentRangeIndicator.GetComponent<RangeIndicator>()
                .Show(tower.GetEffectiveRange());
        }

        if (selectionBoxPrefab != null)
        {
            currentSelectionBox = Instantiate(selectionBoxPrefab, tower.transform);
            currentSelectionBox.transform.localPosition = Vector3.zero;

            Bounds bounds = GetTowerBounds(tower.gameObject);
            currentSelectionBox.GetComponent<SelectionBox>().Show(bounds);
        }
    }

    private void HideVisuals()
    {
        if (currentRangeIndicator != null)
        {
            Destroy(currentRangeIndicator);
            currentRangeIndicator = null;
        }
        if (currentSelectionBox != null)
        {
            Destroy(currentSelectionBox);
            currentSelectionBox = null;
        }
    }

    private Bounds GetTowerBounds(GameObject tower)
    {
        MeshRenderer[] renderers = tower.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return new Bounds(tower.transform.position, Vector3.one);

        Bounds bounds = renderers[0].bounds;
        foreach (MeshRenderer r in renderers)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }

    private void OnRadarSelected(RadarTower radar)
    {
        HideVisuals();

        if (rangeIndicatorPrefab != null)
        {
            currentRangeIndicator = Instantiate(rangeIndicatorPrefab);
            currentRangeIndicator.transform.position = new Vector3(
                radar.transform.position.x,
                radar.transform.position.y,
                radar.transform.position.z
            );
            currentRangeIndicator.GetComponent<RangeIndicator>().Show(radar.buffRadius);
        }

        if (selectionBoxPrefab != null)
        {
            currentSelectionBox = Instantiate(selectionBoxPrefab, radar.transform);
            currentSelectionBox.transform.localPosition = Vector3.zero;
            Bounds bounds = GetTowerBounds(radar.gameObject);
            currentSelectionBox.GetComponent<SelectionBox>().Show(bounds);
        }
    }

    private void OnRadarDeselected()
    {
        HideVisuals();
    }
}
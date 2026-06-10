using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private RectTransform gameAreaRect;

    [Header("Settings")]
    [SerializeField] private LayerMask towerLayer;

    [Header("UI")]
    [SerializeField] private RectTransform upgradePanelRect;

    private CombatTower selectedTower;
    private RadarTower selectedRadar;

    public event System.Action<CombatTower> OnTowerSelected;
    public event System.Action OnTowerDeselected;

    public event System.Action<RadarTower> OnRadarSelected;
    public event System.Action OnRadarDeselected;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameCamera == null) gameCamera = Camera.main;
    }

    void Update()
    {
        HandleSelection();
    }

    private bool IsMouseOverGameArea()
    {
        Vector3[] corners = new Vector3[4];
        gameAreaRect.GetWorldCorners(corners);

        Vector2 mousePos = Input.mousePosition;

        return mousePos.x >= corners[0].x && mousePos.x <= corners[2].x &&
               mousePos.y >= corners[0].y && mousePos.y <= corners[2].y;
    }

    private bool IsMouseOverPanel(RectTransform panel)
    {
        if (panel == null) return false;

        Vector3[] corners = new Vector3[4];
        panel.GetWorldCorners(corners);

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

    void HandleSelection()
    {
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectTower();
            DeselectRadar();
            return;
        }

        if (!IsMouseOverGameArea()) return;
        if (BuildManager.Instance != null && BuildManager.Instance.IsInPlacementMode()) return;
        if (IsMouseOverPanel(upgradePanelRect)) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = GetGameAreaRay();
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, towerLayer))
            {
                CombatTower tower = hit.collider.GetComponentInParent<CombatTower>();
                RadarTower radar = hit.collider.GetComponentInParent<RadarTower>();

                if (tower != null) SelectTower(tower);
                else if (radar != null) SelectRadar(radar);
                else
                {
                    DeselectTower();
                    DeselectRadar(); 
                }
            }
            else
            {
                DeselectTower();
                DeselectRadar(); 
            }
        }
    }

    public void SelectTower(CombatTower tower)
    {
        if (selectedTower == tower) return;
        if (selectedTower != null) DeselectTower();
        if (selectedRadar != null) DeselectRadar(); 

        selectedTower = tower;
        OnTowerSelected?.Invoke(tower);
    }

    public void DeselectTower()
    {
        if (selectedTower == null) return;
        selectedTower = null;
        OnTowerDeselected?.Invoke();
    }
    public void SelectRadar(RadarTower radar)
    {
        if (selectedRadar == radar) return;
        if (selectedTower != null) DeselectTower();
        if (selectedRadar != null) DeselectRadar();

        selectedRadar = radar;
        OnRadarSelected?.Invoke(radar);
    }

    public void DeselectRadar()
    {
        if (selectedRadar == null) return;
        selectedRadar = null;
        OnRadarDeselected?.Invoke();
    }
}
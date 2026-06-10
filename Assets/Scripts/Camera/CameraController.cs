using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Setup")]
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float smoothing = 5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 100f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minHeight = 3f;
    public float maxHeight = 40f;

    [Header("Movement Boundaries")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    private Quaternion targetRotation;
    private Vector3 targetRigPos;
    private float targetHeight;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        targetRotation = transform.rotation;
        targetRigPos = transform.position;
        targetHeight = cameraTransform.localPosition.y;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZoom();
        ApplyMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h == 0f && v == 0f) return;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;
        Vector3 newPos = targetRigPos + moveDir * moveSpeed * Time.deltaTime;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        newPos.y = targetRigPos.y;

        targetRigPos = newPos;
    }

    void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
            targetRotation *= Quaternion.Euler(Vector3.up * rotationSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.E))
            targetRotation *= Quaternion.Euler(Vector3.down * rotationSpeed * Time.deltaTime);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.01f) return;

        float newHeight = targetHeight - scroll * zoomSpeed * 10f;
        targetHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);
    }

    void ApplyMovement()
    {
        transform.position = Vector3.Lerp(
            transform.position, targetRigPos, Time.deltaTime * smoothing);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, targetRotation, Time.deltaTime * smoothing);

        Vector3 camPos = cameraTransform.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetHeight, Time.deltaTime * smoothing);
        cameraTransform.localPosition = camPos;
    }
}
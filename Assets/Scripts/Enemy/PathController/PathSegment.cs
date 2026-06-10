using UnityEngine;

public class PathSegment : MonoBehaviour
{
    public int segmentIndex;

    [Header("Waypoints")]
    public Transform startPoint;
    public Transform endPoint;

    public Vector3 GetStartPoint() => startPoint != null ? startPoint.position : transform.position;
    public Vector3 GetEndPoint() => endPoint != null ? endPoint.position : transform.position;
}
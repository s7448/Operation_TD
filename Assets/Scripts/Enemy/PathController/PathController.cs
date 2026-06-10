using UnityEngine;
using System.Collections.Generic;

public class PathController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;

    private PathSegment[] segments;
    private Transform[] waypoints;

    void Awake()
    {
        BuildPath();
    }

    private void BuildPath()
    {
        segments = GetComponentsInChildren<PathSegment>();
        System.Array.Sort(segments, (a, b) => a.segmentIndex.CompareTo(b.segmentIndex));

        List<Transform> waypointList = new List<Transform>();

        foreach (PathSegment seg in segments)
        {
            if (seg.startPoint != null)
                waypointList.Add(seg.startPoint);
            if (seg.endPoint != null)
                waypointList.Add(seg.endPoint);
        }

        waypoints = waypointList.ToArray();
    }

    public Transform[] GetWaypoints() => waypoints;
    public Transform GetSpawnPoint()
    {
        if (segments != null && segments.Length > 0 && segments[0].startPoint != null)
            return segments[0].startPoint;
        return waypoints != null && waypoints.Length > 0 ? waypoints[0] : null;
    }
    public Transform GetExitPoint() => waypoints != null && waypoints.Length > 0 ? waypoints[waypoints.Length - 1] : null;

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        PathSegment[] segs = GetComponentsInChildren<PathSegment>();
        System.Array.Sort(segs, (a, b) => a.segmentIndex.CompareTo(b.segmentIndex));

        for (int i = 0; i < segs.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(segs[i].GetEndPoint(), 0.4f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(segs[i].GetStartPoint(), 0.4f);

            if (i < segs.Length - 1)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(segs[i].GetEndPoint(), segs[i + 1].GetStartPoint());
            }
        }
    }
}
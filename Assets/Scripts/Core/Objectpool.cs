using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }


    private readonly Dictionary<GameObject, Stack<GameObject>> _free = new();
    private readonly Dictionary<int, GameObject> _source = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!_free.TryGetValue(prefab, out var stack))
            _free[prefab] = stack = new Stack<GameObject>();

        GameObject obj = stack.Count > 0 ? stack.Pop() : Instantiate(prefab);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        _source[obj.GetInstanceID()] = prefab;
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        if (!_source.TryGetValue(obj.GetInstanceID(), out var prefab))
        {
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        _free[prefab].Push(obj);
    }

    // ── Pre-warming (optional) ────────────────────────────────────────────────


    public void Prewarm(GameObject prefab, int count)
    {
        if (!_free.TryGetValue(prefab, out var stack))
            _free[prefab] = stack = new Stack<GameObject>();

        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab);
            _source[obj.GetInstanceID()] = prefab;
            obj.SetActive(false);
            stack.Push(obj);
        }
    }
}

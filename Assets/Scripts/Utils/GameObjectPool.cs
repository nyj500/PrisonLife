using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool
{
    readonly GameObject prefab;
    readonly Transform parent;
    readonly Queue<GameObject> available = new Queue<GameObject>();

    public GameObjectPool(GameObject prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
            available.Enqueue(CreateNew());
    }

    GameObject CreateNew()
    {
        var go = Object.Instantiate(prefab, parent);
        go.SetActive(false);
        return go;
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        var go = available.Count > 0 ? available.Dequeue() : CreateNew();
        go.transform.SetParent(null);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(parent);
        available.Enqueue(go);
    }
}

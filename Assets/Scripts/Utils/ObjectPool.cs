using UnityEngine;
using System.Collections.Generic;

public class ObjectPool<T> where T : MonoBehaviour
{
    readonly T prefab;
    readonly Transform parent;
    readonly Queue<T> pool = new Queue<T>();

    public ObjectPool(T prefab, int initialSize, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        for (int i = 0; i < initialSize; i++)
            pool.Enqueue(CreateNew());
    }

    T CreateNew()
    {
        var obj = Object.Instantiate(prefab, parent);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public T Get(Vector3 position, Quaternion rotation)
    {
        var obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.transform.SetParent(null);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(parent);
        pool.Enqueue(obj);
    }
}

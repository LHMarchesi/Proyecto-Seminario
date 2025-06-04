using System.Collections.Generic;
using UnityEngine;

public class PoolManager<T> where T : Component
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> pool = new Queue<T>();

    public PoolManager(T prefab, int initialSize = 5, Transform parent = null)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        T obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Object.Instantiate(prefab, parent);
        }

        obj.gameObject.SetActive(true);
        if (obj is IPoolable poolable)
            poolable.OnSpawn();
        return obj;
    }

    public void Release(T obj)
    {
        if (obj is IPoolable poolable)
            poolable.OnDespawn();

        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
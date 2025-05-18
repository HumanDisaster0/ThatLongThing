using System.Collections.Generic;
using UnityEngine;

public class RainObjectPool : MonoBehaviour
{
    public GameObject rainPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public GameObject Get()
    {
        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            obj = Instantiate(rainPrefab);
        }

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}

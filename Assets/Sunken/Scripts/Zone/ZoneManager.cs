using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public static ZoneManager instance = null;

    [SerializeField] private List<GameObject> zones = new List<GameObject>();
    private List<bool> zoneStat = new List<bool>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitZone();
    }

    // Start is called before the first frame update
    public void InitZone()
    {
        zoneStat.Capacity = zones.Count;
        foreach(GameObject zone in zones)
        {
            zoneStat.Add(zone.activeSelf);
        }
    }

    public void ResetZone()
    {
        int idx = 0;
        foreach (GameObject zone in zones)
        {
            zone.SetActive(zoneStat[idx]);
            idx++;
        }
    }
}

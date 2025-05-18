using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    static MonsterManager instance = null;

    [SerializeField] MonsterType monsterType = MonsterType.Mole;
    [SerializeField] List<GameObject> monsters = new List<GameObject>();
    [SerializeField] List<bool> activeStat = new List<bool>();
    [SerializeField] List<Vector3> posStat = new List<Vector3>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitMonster();
    }

    public void SetType(MonsterType type)
    {
        monsterType = type;
    }

    public void InitMonster()
    {
        activeStat.Capacity = monsters.Count;
        posStat.Capacity = monsters.Count;

        int idx = 0;
        foreach (GameObject monster in monsters)
        {
            monster.transform.GetComponentInChildren<MMove>().SetType(monsterType);
            activeStat[idx] = monster.transform.GetChild(0).gameObject.activeSelf;
            posStat[idx] = monster.transform.GetChild(0).position;
            idx++;
        }
    }

    public void ResetMonster()
    {
        int idx = 0;
        foreach (GameObject monster in monsters)
        {
            if (activeStat[idx])
                monster.transform.GetChild(0).GetComponent<MMove>().Respawn();
            monster.transform.GetChild(0).position = posStat[idx];
            idx++;
        }
    }
}

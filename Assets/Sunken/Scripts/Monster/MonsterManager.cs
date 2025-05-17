using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [SerializeField] MonsterType monsterType = MonsterType.Mole;
    [SerializeField] List<GameObject> monsters;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject monster in monsters)
        {
            monster.transform.GetComponentInChildren<MMove>().SetType(monsterType);
        }
    }

    public void SetType(MonsterType type)
    {
        monsterType = type;

        Start();
    }
}

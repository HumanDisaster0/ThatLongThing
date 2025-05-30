using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum MSpawnType
{
    Normal = 0,
    Alter,
    NoMatter
}

[Serializable]
public struct MonsterData
{
    public GameObject monster;
    public MonsterType startType;
    public MSpawnType spawnType;
    public Vector3 startPos;
    public bool startActive;
}

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager instance = null;

    [SerializeField] List<MonsterData> monsterDatas = new List<MonsterData>();

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
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            var data = monsterDatas[i];
            data.startType = type;
            data.monster.GetComponentInChildren<MMove>().SetType(type);
            if (data.startType != MonsterType.None)
                data.startActive = true;
            else
                data.startActive = false;
            monsterDatas[i] = data;
        }
    }

    public void SetSpawnType(MSpawnType type)
    {
        foreach(var data in monsterDatas)
        {
            if(data.spawnType != type && data.spawnType != MSpawnType.NoMatter)
                data.monster.SetActive(false);
            else
                data.monster.SetActive(true);
        }

        InitMonster();
    }

    public void SetType(MonsterType type, GameObject monster)
    {
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            if (monster == monsterDatas[i].monster)
            {
                var data = monsterDatas[i];
                data.startType = type;
                data.monster.transform.GetComponentInChildren<MMove>().SetType(type);
                if (data.startType != MonsterType.None)
                    data.startActive = true;
                else
                    data.startActive = false;
                monsterDatas[i] = data;
                return;
            }
        }
    }

    public void InitMonster()
    {
        //for (int i = 0; i < monsterDatas.Count; i++)
        //{
        //    var data = monsterDatas[i];

        //    if(!data.monster.transform.GetComponentInChildren<MMove>())
        //    {
        //        data.startActive = false;
        //        continue;
        //    }
        //    data.monster.transform.GetComponentInChildren<MMove>().SetType(data.startType);
        //    if (data.startType != MonsterType.None)
        //        data.startActive = true;
        //    else
        //        data.startActive = false;
        //    data.startPos = data.monster.transform.GetChild(0).localPosition;

        //    monsterDatas[i] = data; // Update the list with the modified struct
        //}
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            var data = monsterDatas[i];

            if (!data.monster.transform.GetComponentInChildren<MMove>())
            {
                data.startActive = false;
                continue;
            }

            data.monster.transform.GetComponentInChildren<MMove>().SetType(data.startType);

            // 몬스터가 활성화 상태인지 확인하여 startActive 값 설정
            data.startActive = data.monster.activeSelf;

            data.startPos = data.monster.transform.GetChild(0).localPosition;

            monsterDatas[i] = data; // Update the list with the modified struct
        }

    }

    public void ResetMonster()
    {
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            if (monsterDatas[i].monster == null)
                continue;
            if(monsterDatas[i].startActive)
            {
                monsterDatas[i].monster.transform.GetChild(0).gameObject.SetActive(true);
                monsterDatas[i].monster.transform.GetChild(0).GetComponent<MMove>().Respawn();
            }
                
            monsterDatas[i].monster.transform.GetChild(0).localPosition = monsterDatas[i].startPos;
        }
    }

    public void ChangeAllScale(float targetScale)
    {
        for (int i = 0; i < monsterDatas.Count; i++)
        {
            var data = monsterDatas[i];
            
            Vector3 prevScale = data.monster.transform.GetChild(0).localScale;
            float scaleChange = targetScale - prevScale.y;

            data.startPos += new Vector3(0, scaleChange * 0.5f, 0);
            data.monster.transform.GetChild(0).localScale = new Vector2(targetScale, targetScale);
            data.monster.transform.GetChild(0).localPosition = data.startPos; 

            monsterDatas[i] = data; // Update the list with the modified struct
        }
    }

    public void SetAllBehavior(MBehavior behavior)
    {
        foreach(var data in monsterDatas)
        {
            data.monster.transform.GetChild(0).GetComponent<MMove>().SetBehavior(behavior);
        }
    }
}

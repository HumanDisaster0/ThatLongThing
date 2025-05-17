using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType
{
    Normal = 0,
    Alter
}

[Serializable]
public class PlatformPair
{
    public PlatformType type;
    public List<GameObject> tiles;
}

public class PlatformManager : MonoBehaviour
{
    [SerializeField] PlatformType currType;
    [SerializeField] List<PlatformPair> platforms;

    // Start is called before the first frame update
    void Start()
    {
        foreach (PlatformPair pair in platforms)
        {
            if (pair.type == currType)
                SetActive(pair, true);
            else
                SetActive(pair, false);
        }
    }

    void SetActive(PlatformPair _pair, bool _bool)
    {
        foreach (GameObject go in _pair.tiles)
        {
            go.SetActive(_bool);
        }
    }

    public void SetPlatformType(PlatformType _type)
    {
        currType = _type;

        Start();
    }
}

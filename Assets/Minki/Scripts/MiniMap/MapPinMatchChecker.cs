using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Net.NetworkInformation;

public class MapPinMatchChecker : MonoBehaviour
{
    public MapPinSetter setter;
    public RectTransform refRect;
    public Tilemap tilemap;
    public int tileSize = 50;
    public float pinCheckRadius = 0.5f;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    // Start is called before the first frame update
    void Start()
    {
        //월드 좌표 계산을 위한 사전 정보 수집
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        int canvasWidth = (int)refRect.sizeDelta.x;
        int canvasHeight = (int)refRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;
    }


    public bool CheckPinsIsValid()
    {
        foreach (var pin in setter.pins)
        {
            var rect = pin.GetComponent<RectTransform>();
            var anchoredPos = rect.anchoredPosition;

            //월드 좌표 계산
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / tileSize;
            var posY = (anchoredPos.y + (m_maxY * tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / tileSize;


        }

        return false;
    }
}

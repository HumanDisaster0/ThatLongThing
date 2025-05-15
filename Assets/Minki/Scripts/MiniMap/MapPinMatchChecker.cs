using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public struct MapMatchData
{
    public int correct;
    public int wrong;
}

public class MapPinMatchChecker : MonoBehaviour
{
    public MapPinSetter setter;
    public RectTransform refRect;
    public Tilemap tilemap;
    public string checkTag = "TrapInfo";
    public int tileSize = 50;
    public float pinCheckRadius = 0.5f;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    Collider2D[] m_cols = new Collider2D[8];
    HashSet<int> m_duplicatePrevent = new HashSet<int>();

    // Start is called before the first frame update
    void Start()
    {
        //���� ��ǥ ����� ���� ���� ���� ����
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        int canvasWidth = (int)refRect.sizeDelta.x;
        int canvasHeight = (int)refRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            var data = CreateMatchData();

            print($"{data.correct} : {data.wrong}");
        }
           
    }


    public MapMatchData CreateMatchData()
    {
        var data = new MapMatchData();
        m_duplicatePrevent.Clear();

        foreach (var pin in setter.pins)
        {
            var rect = pin.GetComponent<RectTransform>();
            var pinInfo = pin.GetComponent<MapPin>();
            var anchoredPos = rect.anchoredPosition;

            //���� ��ǥ ���
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / tileSize;
            var posY = (anchoredPos.y + (m_maxY * tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / tileSize;

            var trapInfo = TrapInfoOverlap(new Vector2(posX, posY), pinCheckRadius, -1);

            if(trapInfo)
            {
                if (!m_duplicatePrevent.Contains(trapInfo.GetHashCode()))
                    m_duplicatePrevent.Add(trapInfo.GetHashCode());
                else
                    continue;

                //���� ������ �����
                if (trapInfo.type == TrapType.Fine
                    && pinInfo.GetMapPinState == MapPinState.Fine)
                {
                    data.correct++;
                }
                else if (trapInfo.type == TrapType.Danger
                    && pinInfo.GetMapPinState == MapPinState.Danger)
                {
                    data.correct++;
                }
                else if (trapInfo.type == TrapType.Strange
                    && pinInfo.GetMapPinState == MapPinState.Strange)
                {
                    data.correct++;
                }
                else
                {
                    data.wrong++;
                }
            }

            var trapInfos = FindObjectsByType<TrapInfo>(FindObjectsSortMode.None);

            data.wrong += trapInfos.Length - (data.wrong + data.correct);
        }

        return data;
    }


    TrapInfo TrapInfoOverlap(Vector2 point, float radius, LayerMask layerMask)
    {
        //non alloc���� �ش� ����� ��� �浹������ �ݶ��̴� ���� ��������
        var overlapCount = Physics2D.OverlapCircleNonAlloc(point, radius, m_cols);

        //hitcount�� �ϳ��� �ִ��� Ȯ��
        if (overlapCount > 0)
        {
            //���� ����� �ݶ��̴� ã��
            Collider2D nearestCol = null;
            var nearestDist = Mathf.Infinity;
            for (int i = 0; i < overlapCount; i++)
            {
                var currentCol = m_cols[i];

                if (!currentCol)
                    continue;

                if (currentCol.tag != checkTag)
                    continue;

                var dist = Vector3.Distance(currentCol.transform.position, point);

                if (Vector3.Distance(currentCol.transform.position , point) < nearestDist)
                {
                    nearestDist = Vector3.Distance(currentCol.transform.position, point);
                    nearestCol = currentCol;
                }
            }

            return nearestCol.GetComponent<TrapInfo>();
        }

        return null;
    }
}

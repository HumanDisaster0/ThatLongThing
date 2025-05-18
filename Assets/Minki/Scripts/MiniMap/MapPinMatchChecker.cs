using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Unity.VisualScripting;

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
    public float pinCheckRadius = 0.5f;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    Collider2D[] m_cols = new Collider2D[8];
    HashSet<int> m_duplicatePrevent = new HashSet<int>();

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.C))
        //{
        //    var data = CreateMatchData();

        //    print($"{data.correct} : {data.wrong}");
        //}
    }

    // Start is called before the first frame update
    void Start()
    {
        ApplyTileInfo();

        MinimapTileInfo.OnChangedTileSize += ApplyTileInfo;
    }

    private void OnDestroy()
    {
        MinimapTileInfo.OnChangedTileSize -= ApplyTileInfo;
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

            //월드 좌표 계산
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / MinimapTileInfo.tileSize;
            var posY = (anchoredPos.y + (m_maxY * MinimapTileInfo.tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / MinimapTileInfo.tileSize;

            var trapInfo = TrapInfoOverlap(new Vector2(posX, posY), pinCheckRadius, -1);

            if(trapInfo != null)
            {
                if (!m_duplicatePrevent.Contains(trapInfo.GetHashCode()))
                    m_duplicatePrevent.Add(trapInfo.GetHashCode());
                else
                    continue;

                //정산 데이터 만들기
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

            //var trapInfos = FindObjectsByType<TrapInfo>(FindObjectsSortMode.None);
        }
        data.wrong += setter.maxPinCount - (data.wrong + data.correct);

        return data;
    }

    void ApplyTileInfo()
    {
        //월드 좌표 계산을 위한 사전 정보 수집
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        int canvasWidth = (int)refRect.sizeDelta.x;
        int canvasHeight = (int)refRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;
    }


    TrapInfo TrapInfoOverlap(Vector2 point, float radius, LayerMask layerMask)
    {
        //non alloc으로 해당 경로의 모든 충돌가능한 콜라이더 참조 가져오기
        Debug.DrawRay(point,Vector3.up,Color.cyan,2f);
        print(point);
        var overlapCount = Physics2D.OverlapCircleNonAlloc(point, radius, m_cols, (1<<5));

        //hitcount가 하나라도 있는지 확인
        if (overlapCount > 0)
        {
            print(overlapCount);

            //가장 가까운 콜라이더 찾기
            Collider2D nearestCol = null;
            var nearestDist = Mathf.Infinity;
            for (int i = 0; i < overlapCount; i++)
            {
                var currentCol = m_cols[i];

                if (currentCol == null)
                {
                    print("null");
                    continue;
                }
                   

                if (!currentCol.CompareTag(checkTag))
                {
                    print($"not tag : {currentCol.tag}");
                    continue;
                }
                   

                var dist = Vector3.Distance(currentCol.transform.position, point);

                if (Vector3.Distance(currentCol.transform.position , point) < nearestDist)
                {
                    print(currentCol.name);
                    nearestDist = Vector3.Distance(currentCol.transform.position, point);
                    nearestCol = currentCol;
                }
            }

            return nearestCol?.GetComponent<TrapInfo>();
        }

        return null;
    }
}

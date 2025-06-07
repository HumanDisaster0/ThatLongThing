using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public enum MatchState
{
    None = 0,
    Correct,
    Wrong,
    Ambiguity
}

public struct MapMatchData
{
    public MatchState state;
    public Vector3 position;
    public MapMatchData(MatchState state = MatchState.None, Vector3 position = default)
    {
        this.state = state;
        this.position = position;
    }
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
    Dictionary<int,MapMatchData> m_currentTraps = new Dictionary<int, MapMatchData>();

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

    public List<MapMatchData> CreateMatchData()
    {
        var data = new List<MapMatchData>();

        m_currentTraps.Clear();

        //트랩 정보 초기화
        foreach (var item in FindObjectsByType<TrapSetter>(FindObjectsSortMode.None))
        {
            m_currentTraps.Add(item.gameObject.GetInstanceID(),new MapMatchData(MatchState.None, item.transform.position));
        }

        // 오토 핀 체크
        foreach (var pinInfo in FindObjectsByType<StaticMapPin>(FindObjectsSortMode.None))
        {
            var rect = pinInfo.GetComponent<RectTransform>();
            var anchoredPos = rect.anchoredPosition;

            //월드 좌표 계산
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / MinimapTileInfo.tileSize;
            var posY = (anchoredPos.y + (m_maxY * MinimapTileInfo.tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / MinimapTileInfo.tileSize;

            var trapInfo = TrapInfoOverlap(new Vector2(posX, posY), pinCheckRadius, -1);

            if(trapInfo != null)
            {
                var checkTrap = m_currentTraps[trapInfo.gameObject.GetInstanceID()];
                switch (checkTrap.state)
                {
                    case MatchState.None:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.MapPinState)
                        {
                            //트랩이 핀과 일치하는 경우
                            //맞음
                            checkTrap.state = MatchState.Correct;
                        }
                        else
                        {
                            //트랩이 핀과 일치하지 않는 경우
                            //틀림
                            checkTrap.state = MatchState.Wrong;
                        }
                        break;
                    case MatchState.Correct:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) != pinInfo.MapPinState)
                        {
                            //이미 맞은 트랩이 핀과 일치하지 않는 경우
                            checkTrap.state = MatchState.Ambiguity; //애매모호 상태로 변경
                        }
                        break;
                    case MatchState.Wrong:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.MapPinState)
                        {
                            checkTrap.state = MatchState.Ambiguity; //애매모호 상태로 변경
                        }
                        break;
                }
            }
        }

        // 유저 핀 체크
        foreach (var pin in setter.pins)
        {
            var rect = pin.GetComponent<RectTransform>();
            var pinInfo = pin.GetComponent<MapPin>();
            var anchoredPos = rect.anchoredPosition;

            //월드 좌표 계산
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / MinimapTileInfo.tileSize;
            var posY = (anchoredPos.y + (m_maxY * MinimapTileInfo.tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / MinimapTileInfo.tileSize;

            var trapInfo = TrapInfoOverlap(new Vector2(posX, posY), pinCheckRadius, -1);
            if (trapInfo != null)
            {
                var checkTrap = m_currentTraps[trapInfo.gameObject.GetInstanceID()];
                switch (checkTrap.state)
                {
                    case MatchState.None:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.GetMapPinState)
                        {
                            //트랩이 핀과 일치하는 경우
                            //맞음
                            checkTrap.state = MatchState.Correct;
                        }
                        else
                        {
                            //트랩이 핀과 일치하지 않는 경우
                            //틀림
                            checkTrap.state = MatchState.Wrong;
                        }
                        break;
                    case MatchState.Correct:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) != pinInfo.GetMapPinState)
                        {
                            //이미 맞은 트랩이 핀과 일치하지 않는 경우
                            checkTrap.state = MatchState.Ambiguity; //애매모호 상태로 변경
                        }
                        break;
                    case MatchState.Wrong:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.GetMapPinState)
                        {
                            checkTrap.state = MatchState.Ambiguity; //애매모호 상태로 변경
                        }
                        break;
                }
            }
        }

        //최종 정보 반환
        data = m_currentTraps.Values.ToList();

        data.Sort((a, b) =>
        {
            // 1. x축 값으로 먼저 비교 (작은 것이 앞으로)
            if (a.position.x != b.position.x)
            {
                return a.position.x.CompareTo(b.position.x); // a.x가 b.x보다 작으면 -1, 같으면 0, 크면 1 반환
            }
            else
            {
                // 2. x축 값이 같으면 y축 값으로 비교 (큰 것이 앞으로)
                return b.position.y.CompareTo(a.position.y); // b.y가 a.y보다 작으면 -1 (즉, a.y가 더 크면), 같으면 0, 크면 1 반환
            }
        });

        return data;
    }

    void ApplyTileInfo()
    {
        //월드 좌표 계산을 위한 사전 정보 수집
        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        int canvasWidth = (int)refRect.sizeDelta.x;
        int canvasHeight = (int)refRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;
    }

    MapPinState ConvertTrapTypeToMapPinState(TrapType trapType)
    {
        switch (trapType)
        {
            case TrapType.Fine:
                return MapPinState.Fine;
            case TrapType.Danger:
                return MapPinState.Danger;
            case TrapType.Strange:
                return MapPinState.Strange;
            default:
                return MapPinState.Danger; //기본값
        }
    }


    TrapInfo TrapInfoOverlap(Vector2 point, float radius, LayerMask layerMask)
    {
        //non alloc으로 해당 경로의 모든 충돌가능한 콜라이더 참조 가져오기
        var overlapCount = Physics2D.OverlapCircleNonAlloc(point, radius, m_cols, (1<<5));

        //hitcount가 하나라도 있는지 확인
        if (overlapCount > 0)
        {
            //가장 가까운 콜라이더 찾기
            Collider2D nearestCol = null;
            var nearestDist = Mathf.Infinity;
            for (int i = 0; i < overlapCount; i++)
            {
                var currentCol = m_cols[i];

                if (currentCol == null)
                {
                    continue;
                }
                   

                if (!currentCol.CompareTag(checkTag))
                {
                    continue;
                }
                   

                var dist = Vector3.Distance(currentCol.transform.position, point);

                if (Vector3.Distance(currentCol.transform.position , point) < nearestDist)
                {
                    nearestDist = Vector3.Distance(currentCol.transform.position, point);
                    nearestCol = currentCol;
                }
            }

            return nearestCol?.GetComponent<TrapInfo>();
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!setter)
            return;
        //tilemap.CompressBounds();
        //var b = tilemap.cellBounds;

        //Gizmos.DrawSphere(b.max, 0.5f);
        //Gizmos.DrawSphere(b.min, 0.5f);

        //Debug.Log(b.max);
        //Debug.Log(b.min);

        foreach (var pin in setter.pins)
        {
            var rect = pin.GetComponent<RectTransform>();
            var pinInfo = pin.GetComponent<MapPin>();
            var anchoredPos = rect.anchoredPosition;

            //월드 좌표 계산
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / MinimapTileInfo.tileSize;
            var posY = (anchoredPos.y + (m_maxY * MinimapTileInfo.tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / MinimapTileInfo.tileSize;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(posX, posY, 0), pinCheckRadius);
        }
    }
#endif
}


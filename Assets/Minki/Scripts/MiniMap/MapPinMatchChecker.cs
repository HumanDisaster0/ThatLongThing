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

        //Ʈ�� ���� �ʱ�ȭ
        foreach (var item in FindObjectsByType<TrapSetter>(FindObjectsSortMode.None))
        {
            m_currentTraps.Add(item.gameObject.GetInstanceID(),new MapMatchData(MatchState.None, item.transform.position));
        }

        // ���� �� üũ
        foreach (var pinInfo in FindObjectsByType<StaticMapPin>(FindObjectsSortMode.None))
        {
            var rect = pinInfo.GetComponent<RectTransform>();
            var anchoredPos = rect.anchoredPosition;

            //���� ��ǥ ���
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
                            //Ʈ���� �ɰ� ��ġ�ϴ� ���
                            //����
                            checkTrap.state = MatchState.Correct;
                        }
                        else
                        {
                            //Ʈ���� �ɰ� ��ġ���� �ʴ� ���
                            //Ʋ��
                            checkTrap.state = MatchState.Wrong;
                        }
                        break;
                    case MatchState.Correct:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) != pinInfo.MapPinState)
                        {
                            //�̹� ���� Ʈ���� �ɰ� ��ġ���� �ʴ� ���
                            checkTrap.state = MatchState.Ambiguity; //�ָŸ�ȣ ���·� ����
                        }
                        break;
                    case MatchState.Wrong:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.MapPinState)
                        {
                            checkTrap.state = MatchState.Ambiguity; //�ָŸ�ȣ ���·� ����
                        }
                        break;
                }
            }
        }

        // ���� �� üũ
        foreach (var pin in setter.pins)
        {
            var rect = pin.GetComponent<RectTransform>();
            var pinInfo = pin.GetComponent<MapPin>();
            var anchoredPos = rect.anchoredPosition;

            //���� ��ǥ ���
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
                            //Ʈ���� �ɰ� ��ġ�ϴ� ���
                            //����
                            checkTrap.state = MatchState.Correct;
                        }
                        else
                        {
                            //Ʈ���� �ɰ� ��ġ���� �ʴ� ���
                            //Ʋ��
                            checkTrap.state = MatchState.Wrong;
                        }
                        break;
                    case MatchState.Correct:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) != pinInfo.GetMapPinState)
                        {
                            //�̹� ���� Ʈ���� �ɰ� ��ġ���� �ʴ� ���
                            checkTrap.state = MatchState.Ambiguity; //�ָŸ�ȣ ���·� ����
                        }
                        break;
                    case MatchState.Wrong:
                        if (ConvertTrapTypeToMapPinState(trapInfo.type) == pinInfo.GetMapPinState)
                        {
                            checkTrap.state = MatchState.Ambiguity; //�ָŸ�ȣ ���·� ����
                        }
                        break;
                }
            }
        }

        //���� ���� ��ȯ
        data = m_currentTraps.Values.ToList();

        data.Sort((a, b) =>
        {
            // 1. x�� ������ ���� �� (���� ���� ������)
            if (a.position.x != b.position.x)
            {
                return a.position.x.CompareTo(b.position.x); // a.x�� b.x���� ������ -1, ������ 0, ũ�� 1 ��ȯ
            }
            else
            {
                // 2. x�� ���� ������ y�� ������ �� (ū ���� ������)
                return b.position.y.CompareTo(a.position.y); // b.y�� a.y���� ������ -1 (��, a.y�� �� ũ��), ������ 0, ũ�� 1 ��ȯ
            }
        });

        return data;
    }

    void ApplyTileInfo()
    {
        //���� ��ǥ ����� ���� ���� ���� ����
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
                return MapPinState.Danger; //�⺻��
        }
    }


    TrapInfo TrapInfoOverlap(Vector2 point, float radius, LayerMask layerMask)
    {
        //non alloc���� �ش� ����� ��� �浹������ �ݶ��̴� ���� ��������
        var overlapCount = Physics2D.OverlapCircleNonAlloc(point, radius, m_cols, (1<<5));

        //hitcount�� �ϳ��� �ִ��� Ȯ��
        if (overlapCount > 0)
        {
            //���� ����� �ݶ��̴� ã��
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

            //���� ��ǥ ���
            var posX = (anchoredPos.x - m_pivotX + (rect.sizeDelta.x * 0.5f)) / MinimapTileInfo.tileSize;
            var posY = (anchoredPos.y + (m_maxY * MinimapTileInfo.tileSize) + m_pivotY - (rect.sizeDelta.y * 0.5f)) / MinimapTileInfo.tileSize;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(posX, posY, 0), pinCheckRadius);
        }
    }
#endif
}


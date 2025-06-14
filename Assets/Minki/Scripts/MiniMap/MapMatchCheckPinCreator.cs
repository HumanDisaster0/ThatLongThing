using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapMatchCheckPinCreator : MonoBehaviour
{
    public GameObject MatchCheckPinPrefab;
    public RectTransform MatchCheckPinParent;

    public Tilemap MinimapTilemap;
    public RectTransform RefRect;
    int m_pivotX;
    int m_pivotY;
    int m_maxY;

    void Start()
    {
        MatchCheckPinParent = GetComponent<RectTransform>();
        ApplyTileInfo();

        MinimapTileInfo.OnChangedTileSize += ApplyTileInfo;
    }

    void ApplyTileInfo()
    {
        //월드 좌표 계산을 위한 사전 정보 수집
        MinimapTilemap.CompressBounds();
        BoundsInt bounds = MinimapTilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        int canvasWidth = (int)RefRect.sizeDelta.x;
        int canvasHeight = (int)RefRect.sizeDelta.y;

        m_pivotX = System.Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = System.Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = MinimapTilemap.cellBounds.max.y;
    }

    public void UpdateSibling()
    {
        transform.SetAsLastSibling();
    }

    public void CreatePins(List<MapMatchData> datas)
    {
        foreach (MapMatchData matchData in datas)
        {
            if(matchData.state == MatchState.Correct)
            {
                //맞은 정보를 미니맵에 핀으로 표시
                var pinGO = Instantiate(MatchCheckPinPrefab, MatchCheckPinParent);
                var pinRect = pinGO.GetComponent<RectTransform>();

                pinRect.anchoredPosition = new Vector2(
                    (matchData.position.x * MinimapTileInfo.tileSize) + m_pivotX - (pinRect.sizeDelta.x * 0.5f),
                    (matchData.position.y * MinimapTileInfo.tileSize) - (m_maxY * MinimapTileInfo.tileSize) - m_pivotY + (pinRect.sizeDelta.y * 0.5f)
                );
            }
        }
    }
}

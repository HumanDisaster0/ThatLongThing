using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MinimapPlayerPos : MonoBehaviour
{
    public RectTransform parentRect;
    public Tilemap tilemap;
    public int xOffset = 0;
    public int yOffset = 0;

    public Transform Player;

    public float playerHeightOffset = 0.0f;

    RectTransform m_rect;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    // Start is called before the first frame update
    void Start()
    {
        ApplyTileInfo();
        MinimapTileInfo.OnChangedTileSize += ApplyTileInfo;

        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player").transform;
        }

        m_rect.anchoredPosition = new Vector2(m_pivotX + (Mathf.Abs(Player.position.x + xOffset)) * MinimapTileInfo.tileSize - (m_rect.sizeDelta.x * 0.5f), 
            m_pivotY * -1f - (m_maxY * MinimapTileInfo.tileSize) + (Player.position.y + yOffset) * MinimapTileInfo.tileSize + (m_rect.sizeDelta.y * 0.5f) + playerHeightOffset);
    }

    // Update is called once per frame
    void Update()
    {
        m_rect.anchoredPosition = new Vector2(m_pivotX + (Mathf.Abs(Player.position.x + xOffset)) * MinimapTileInfo.tileSize - (m_rect.sizeDelta.x * 0.5f), 
            m_pivotY * -1f - (m_maxY * MinimapTileInfo.tileSize) + (Player.position.y + yOffset) * MinimapTileInfo.tileSize + (m_rect.sizeDelta.y * 0.5f) + playerHeightOffset);
    }

    private void OnDestroy()
    {
        MinimapTileInfo.OnChangedTileSize -= ApplyTileInfo;
    }

    void ApplyTileInfo()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        m_rect = GetComponent<RectTransform>();

        int canvasWidth = (int)parentRect.sizeDelta.x;
        int canvasHeight = (int)parentRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;
    }
}

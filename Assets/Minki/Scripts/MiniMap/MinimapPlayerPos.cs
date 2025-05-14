using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class MinimapPlayerPos : MonoBehaviour
{
    public RectTransform parentRect;
    public Tilemap tilemap;
    public int tileSize = 50;

    public Transform Player;

    RectTransform m_rect;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    // Start is called before the first frame update
    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        m_rect = GetComponent<RectTransform>();

        int canvasWidth = (int)parentRect.sizeDelta.x;
        int canvasHeight = (int)parentRect.sizeDelta.y;

        m_pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = tilemap.cellBounds.max.y;

        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player").transform;
        }

        m_rect.anchoredPosition = new Vector2(m_pivotX + Player.position.x * tileSize - (m_rect.sizeDelta.x * 0.5f), m_pivotY * -1f - (m_maxY * tileSize) + (Player.position.y * tileSize) + (m_rect.sizeDelta.y * 0.5f));
    }

    // Update is called once per frame
    void Update()
    {
        m_rect.anchoredPosition = new Vector2(m_pivotX + Player.position.x * tileSize - (m_rect.sizeDelta.x * 0.5f), m_pivotY * -1f - (m_maxY * tileSize) + (Player.position.y * tileSize) + (m_rect.sizeDelta.y * 0.5f));
    }
}

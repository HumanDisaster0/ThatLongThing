using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using System;
using UnityEditor.Rendering;

public class MinimapPlayerPos : MonoBehaviour
{
    public RectTransform parentRect;
    public RectTransform contentRect;
    public ScrollRect scrollRect;
    public Tilemap tilemap;
    public int xOffset = 0;
    public int yOffset = 0;

    public bool flickering = true;

    public Transform Player;
    public float flickeringRate = 0.8f;
    public float playerHeightOffset = 0.0f;

    RectTransform m_rect;
    Image m_image;

    bool m_AlphaColIsZero;

    int m_pivotX;
    int m_pivotY;

    int m_maxY;

    float m_flickeringTimer;

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

        m_image = GetComponent<Image>();

        var t = (m_rect.anchoredPosition.x + (m_rect.sizeDelta.x * 0.5f)) / contentRect.sizeDelta.x;
        var clampedT = Mathf.Clamp(t, 0.25f, 0.75f);
        var normalizedT = (clampedT - 0.25f) / (0.5f);

        scrollRect.horizontalNormalizedPosition = normalizedT;
    }

    // Update is called once per frame
    void Update()
    {
        m_rect.anchoredPosition = new Vector2(m_pivotX + (Mathf.Abs(Player.position.x + xOffset)) * MinimapTileInfo.tileSize - (m_rect.sizeDelta.x * 0.5f), 
            m_pivotY * -1f - (m_maxY * MinimapTileInfo.tileSize) + (Player.position.y + yOffset) * MinimapTileInfo.tileSize + (m_rect.sizeDelta.y * 0.5f) + playerHeightOffset);

        if(flickering)
        {
            m_flickeringTimer += Time.deltaTime;

            if (m_flickeringTimer > flickeringRate)
            {
                m_flickeringTimer -= flickeringRate;
                m_AlphaColIsZero = !m_AlphaColIsZero;
            }
        }
        else
        {
            m_AlphaColIsZero = false;
        }

        m_image.color = new Color(1.0f, 1.0f, 1.0f, m_AlphaColIsZero ? 0.0f : 1.0f);

    }

    private void OnEnable()
    {
        if (Player == null)
            return;

        m_rect.anchoredPosition = new Vector2(m_pivotX + (Mathf.Abs(Player.position.x + xOffset)) * MinimapTileInfo.tileSize - (m_rect.sizeDelta.x * 0.5f),
            m_pivotY * -1f - (m_maxY * MinimapTileInfo.tileSize) + (Player.position.y + yOffset) * MinimapTileInfo.tileSize + (m_rect.sizeDelta.y * 0.5f) + playerHeightOffset);

        var t = (m_rect.anchoredPosition.x + (m_rect.sizeDelta.x * 0.5f)) / contentRect.sizeDelta.x;
        var clampedT = Mathf.Clamp(t, 0.25f, 0.75f);
        var normalizedT = (clampedT - 0.25f) / (0.5f);

        scrollRect.horizontalNormalizedPosition = normalizedT;
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

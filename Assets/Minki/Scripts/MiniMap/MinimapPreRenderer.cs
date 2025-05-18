using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Build;

public static class MinimapTileInfo
{
    public static int tileSize
    {
        get
        {
            return m_tileSize;
        }

        set
        {
            if(m_tileSize != value)
            {
                m_tileSize = value;
                OnChangedTileSize?.Invoke();
            }
        }
    }
    public const int DEFAULT_SIZE = 50;

    static int m_tileSize = DEFAULT_SIZE;

    public static Action OnChangedTileSize;
}

public class MinimapPreRenderer : MonoBehaviour
{
    public RectTransform parentRect;
    public ScrollRect scrollRect;
    public Tilemap tilemap;
    public RawImage rawImage;

    void Start()
    {
        if (m_resizedTexture == null)
            m_resizedTexture = new Dictionary<int, Texture2D>();

        MinimapTileInfo.tileSize = MinimapTileInfo.DEFAULT_SIZE;

        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        if (texHeight > 863)
        {
            MinimapTileInfo.tileSize = 34; 
            texWidth = bounds.size.x * MinimapTileInfo.tileSize;
            texHeight = bounds.size.y * MinimapTileInfo.tileSize;
        }
           

        int canvasWidth = (int)parentRect.sizeDelta.x;
        int canvasHeight = (int)parentRect.sizeDelta.y;

        int pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        int pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        Texture2D tex = new Texture2D(pivotX + texWidth, texHeight + pivotY, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // 투명으로 초기화
        Color clear = new Color(0, 0, 0, 0);
        Color[] allClearPixels = new Color[tex.width * tex.height];
        for (int i = 0; i < allClearPixels.Length; i++) allClearPixels[i] = clear;
        tex.SetPixels(0, 0, tex.width, tex.height, allClearPixels);

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                Vector3Int tilePos = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                
                Tile tile = tilemap.GetTile<Tile>(tilePos);
                Sprite sprite = tile?.sprite;

                int px = x * MinimapTileInfo.tileSize + pivotX;
                int py = y * MinimapTileInfo.tileSize;

              

                if (sprite != null)
                {
                    Texture2D sourceTex = sprite.texture;
                    Rect rect = sprite.textureRect;
                    int hash = HashCode.Combine(MinimapTileInfo.tileSize, sourceTex.GetHashCode());
                    if (!m_resizedTexture.ContainsKey(hash))
                    {
                        int rx = Mathf.FloorToInt(rect.x);
                        int ry = Mathf.FloorToInt(rect.y);
                        int rw = Mathf.FloorToInt(rect.width);
                        int rh = Mathf.FloorToInt(rect.height);

                        Color[] spritePixels = sourceTex.GetPixels(rx, ry, rw, rh);

                        // 리사이즈 필요 시 처리
                        Texture2D resized = new Texture2D(rw, rh, TextureFormat.RGBA32, false);
                        resized.SetPixels(spritePixels);
                        resized.Apply();

                        Texture2D scaled = Resize(resized, MinimapTileInfo.tileSize, FilterMode.Bilinear); // 앞서 만든 Resize 함수

                        m_resizedTexture.Add(hash, scaled);
                    }

                    Color[] finalPixels = m_resizedTexture[hash].GetPixels();

                    tex.SetPixels(px, py, MinimapTileInfo.tileSize, MinimapTileInfo.tileSize, finalPixels);
                }
            }
        }

        tex.Apply();

        rawImage.texture = tex;
        rawImage.color = Color.white;

        // 필요 시 크기 조정
        rawImage.rectTransform.sizeDelta = new Vector2(pivotX + texWidth, texHeight + pivotY);

        scrollRect.horizontalScrollbar.value = 0;
    }

    Texture2D Resize(Texture2D src, int newSize, FilterMode filter)
    {
        // 임시 RenderTexture를 원하는 보간으로 준비
        RenderTexture rt = RenderTexture.GetTemporary(
            newSize, newSize,           // 크기
            0,                          // 깊이
            RenderTextureFormat.ARGB32
        );
        rt.filterMode = filter;        // Point, Bilinear, Trilinear 중 택

        // GPU-사이드에서 스케일링
        Graphics.Blit(src, rt);

        // 다시 CPU로 읽어오기
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D dst = new Texture2D(newSize, newSize, TextureFormat.RGBA32, false);
        dst.ReadPixels(new Rect(0, 0, newSize, newSize), 0, 0);
        dst.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return dst;
    }

    static Dictionary<int, Texture2D> m_resizedTexture;
}
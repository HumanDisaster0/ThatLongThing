using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

public static class MinimapTileInfo
{
    public static int tileSize = 50;
}

public class MinimapPreRenderer : MonoBehaviour
{
    public RectTransform parentRect;
    public ScrollRect scrollRect;
    public Tilemap tilemap;
    public RawImage rawImage;

    void Start()
    {
        MinimapTileInfo.tileSize = 50;

        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        if (texHeight > 863)
        {
            MinimapTileInfo.tileSize = 25; 
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
                TileBase tile = tilemap.GetTile(tilePos);

                int px = x * MinimapTileInfo.tileSize + pivotX;
                int py = y * MinimapTileInfo.tileSize;

                if (tile != null)
                {
                    // 예시 색상: 타일마다 고정된 색상으로 칠함
                    Color tileColor = Color.green;

                    switch (tile.name)
                    {
                        case "none_tile":
                            tileColor = new Color(0.78f, 0.78f, 0.78f);
                            break;
                        case "trap_tile":
                            tileColor = new Color(241.0f/255.0f,95.0f /255.0f,95.0f /255.0f);
                            break;
                    }

                    Color[] pixels = new Color[MinimapTileInfo.tileSize * MinimapTileInfo.tileSize];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = tileColor;
                    tex.SetPixels(px, py, MinimapTileInfo.tileSize, MinimapTileInfo.tileSize, pixels);
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
}
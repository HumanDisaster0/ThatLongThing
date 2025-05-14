using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TilemapRenderer : MonoBehaviour
{
    public Tilemap tilemap;
    public RawImage rawImage;

    public int tileSize = 50; // 타일 하나당 픽셀 크기

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // 투명으로 초기화
        Color clear = new Color(0, 0, 0, 0);
        Color[] clearPixels = new Color[tileSize * tileSize];
        for (int i = 0; i < clearPixels.Length; i++) clearPixels[i] = clear;

        for (int y = 0; y < bounds.size.y; y++)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                Vector3Int tilePos = new Vector3Int(bounds.x + x, bounds.y + y, 0);
                TileBase tile = tilemap.GetTile(tilePos);

                int px = x * tileSize;
                int py = y * tileSize;

                if (tile != null)
                {
                    // 예시 색상: 타일마다 고정된 색상으로 칠함
                    Color tileColor = Color.green;
                    Color[] pixels = new Color[tileSize * tileSize];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = tileColor;
                    tex.SetPixels(px, py, tileSize, tileSize, pixels);
                }
                else
                {
                    tex.SetPixels(px, py, tileSize, tileSize, clearPixels);
                }
            }
        }

        tex.Apply();

        rawImage.texture = tex;
        rawImage.color = Color.white;

        // 필요 시 크기 조정
        rawImage.rectTransform.sizeDelta = new Vector2(texWidth, texHeight);
    }
}
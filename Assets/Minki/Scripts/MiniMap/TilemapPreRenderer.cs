using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

public class TilemapPreRenderer : MonoBehaviour
{
    public RectTransform parentRect;
    public Tilemap tilemap;
    public RawImage rawImage;

    public int tileSize = 50; // Ÿ�� �ϳ��� �ȼ� ũ��

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        int canvasWidth = (int)parentRect.sizeDelta.x;
        int canvasHeight = (int)parentRect.sizeDelta.y;

        int pivotX = Math.Max(0, (canvasWidth - texWidth) / 2);
        int pivotY = Math.Max(0, (canvasHeight - texHeight) / 2);

        Texture2D tex = new Texture2D(pivotX + texWidth, texHeight + pivotY, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // �������� �ʱ�ȭ
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

                int px = x * tileSize + pivotX;
                int py = y * tileSize;

                if (tile != null)
                {
                    // ���� ����: Ÿ�ϸ��� ������ �������� ĥ��
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

                    Color[] pixels = new Color[tileSize * tileSize];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = tileColor;
                    tex.SetPixels(px, py, tileSize, tileSize, pixels);
                }
            }
        }

        tex.Apply();

        rawImage.texture = tex;
        rawImage.color = Color.white;

        // �ʿ� �� ũ�� ����
        rawImage.rectTransform.sizeDelta = new Vector2(pivotX + texWidth, texHeight + pivotY);
    }
}
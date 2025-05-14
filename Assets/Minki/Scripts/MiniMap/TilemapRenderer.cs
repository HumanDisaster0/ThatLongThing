using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class TilemapRenderer : MonoBehaviour
{
    public Tilemap tilemap;
    public RawImage rawImage;

    public int tileSize = 50; // Ÿ�� �ϳ��� �ȼ� ũ��

    void Start()
    {
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * tileSize;
        int texHeight = bounds.size.y * tileSize;

        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        // �������� �ʱ�ȭ
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
                    // ���� ����: Ÿ�ϸ��� ������ �������� ĥ��
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

        // �ʿ� �� ũ�� ����
        rawImage.rectTransform.sizeDelta = new Vector2(texWidth, texHeight);
    }
}
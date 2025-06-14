using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;

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

    private bool m_hasInit = false;

    public void InitializeRenderer()
    {
        if (m_hasInit)
            return;

        m_hasInit = true;

        if (m_resizedTexture == null)
            m_resizedTexture = new Dictionary<int, Texture2D>();

        MinimapTileInfo.tileSize = MinimapTileInfo.DEFAULT_SIZE;

        tilemap.CompressBounds();
        BoundsInt bounds = tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        if (texHeight > 812)
        {
            MinimapTileInfo.tileSize = 26;
            texWidth = bounds.size.x * MinimapTileInfo.tileSize;
            texHeight = bounds.size.y * MinimapTileInfo.tileSize;
        }


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

                Tile tile = tilemap.GetTile<Tile>(tilePos);
                Sprite sprite = tile?.sprite;

                int px = x * MinimapTileInfo.tileSize + pivotX;
                int py = y * MinimapTileInfo.tileSize;



                if (sprite != null)
                {
                    Texture2D sourceTex = sprite.texture;
                    Rect rect = sprite.rect;

                    int fullSize = Mathf.Max((int)rect.width, (int)rect.height);
                    int hash = HashCode.Combine(MinimapTileInfo.tileSize, sourceTex.name);

                    if (!m_resizedTexture.ContainsKey(hash))
                    {
                        // ��������Ʈ���� ���� �ȼ� ��������
                        int rx = Mathf.FloorToInt(rect.x);
                        int ry = Mathf.FloorToInt(rect.y);
                        int rw = Mathf.FloorToInt(rect.width);
                        int rh = Mathf.FloorToInt(rect.height);

                        Color[] spritePixels = sourceTex.GetPixels(rx, ry, rw, rh);

                        // �� ���簢�� �ؽ�ó ���� �� ��ü�� �������� �ʱ�ȭ
                        Texture2D paddedTex = new Texture2D(fullSize, fullSize, TextureFormat.RGBA32, false);
                        Color[] clearPixels = Enumerable.Repeat(new Color(0, 0, 0, 0), fullSize * fullSize).ToArray();
                        paddedTex.SetPixels(clearPixels);

                        // �߽� �����ؼ� ��������Ʈ ����
                        int offsetX = (fullSize - rw) / 2;
                        int offsetY = (fullSize - rh) / 2;
                        paddedTex.SetPixels(offsetX, offsetY, rw, rh, spritePixels);
                        paddedTex.Apply();

                        // ���ϴ� ũ��� ��������
                        Texture2D scaled = Resize(paddedTex, MinimapTileInfo.tileSize, FilterMode.Bilinear);
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

        // �ʿ� �� ũ�� ����
        rawImage.rectTransform.sizeDelta = new Vector2(pivotX + texWidth, texHeight + pivotY);

        scrollRect.horizontalScrollbar.value = 0;
    }

    void Awake()
    {
        InitializeRenderer();
    }

    Texture2D Resize(Texture2D src, int newSize, FilterMode filter)
    {
        // �ӽ� RenderTexture�� ���ϴ� �������� �غ�
        RenderTexture rt = RenderTexture.GetTemporary(
            newSize, newSize,           // ũ��
            0,                          // ����
            RenderTextureFormat.ARGB32
        );
        rt.filterMode = filter;        // Point, Bilinear, Trilinear �� ��

        // GPU-���̵忡�� �����ϸ�
        Graphics.Blit(src, rt);

        // �ٽ� CPU�� �о����
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
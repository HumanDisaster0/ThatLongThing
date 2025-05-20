using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SpriteTile
{
    public Sprite sprite;
    public Vector2 scale = Vector2.one;
    public float rotationZ = 0f;
}

[CreateAssetMenu(menuName = "UI/ChatBubbleStyle")]
public class ChatBubbleStyle : ScriptableObject
{
    public SpriteTile cornerTopLeft;
    public SpriteTile cornerTopRight;
    public SpriteTile cornerBottomLeft;
    public SpriteTile cornerBottomRight;

    public SpriteTile edgeTop;
    public SpriteTile edgeBottom;
    public SpriteTile edgeLeft;
    public SpriteTile edgeRight;

    public SpriteTile centerTile;
    public SpriteTile decorationTileA;
    public SpriteTile decorationTileB;
}

public class ChatBubbleBuilder : MonoBehaviour
{
    [Header("스타일 데이터")]
    public ChatBubbleStyle style;

    [Header("Layout Holder")]
    public Transform holder;

    [Header("설정값")]
    public int width = 10;
    public int height = 4;
    public int decorationCount = 2;

    private List<GameObject> pooledTiles = new();
    private List<GameObject> pooledRows = new();

    private GameObject GetRow()
    {
        foreach (var row in pooledRows)
        {
            if (!row.activeSelf)
            {
                row.SetActive(true);
                return row;
            }
        }

        GameObject newRow = new GameObject("Row", typeof(RectTransform));
        newRow.transform.SetParent(holder);
        HorizontalLayoutGroup layout = newRow.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 0;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        pooledRows.Add(newRow);
        return newRow;
    }

    private GameObject GetTile(SpriteTile tileData)
    {
        foreach (var tile in pooledTiles)
        {
            if (!tile.activeSelf && tile.name.StartsWith(tileData.sprite.name))
            {
                tile.SetActive(true);
                SetImage(tile, tileData);
                return tile;
            }
        }

        GameObject newTile = new GameObject(tileData.sprite.name + "_Clone", typeof(RectTransform), typeof(Image));
        newTile.transform.SetParent(holder);
        SetImage(newTile, tileData);
        pooledTiles.Add(newTile);
        return newTile;
    }

    private void SetImage(GameObject go, SpriteTile tileData)
    {
        Image img = go.GetComponent<Image>();
        img.sprite = tileData.sprite;
        go.transform.localScale = new Vector3(tileData.scale.x, tileData.scale.y, 1f);
        go.transform.rotation = Quaternion.Euler(0f, 0f, tileData.rotationZ);
    }

    public void ApplyStyle(ChatBubbleStyle newStyle)
    {
        style = newStyle;
    }

    public void BuildBubble()
    {
        if (style == null)
        {
            Debug.LogWarning("ChatBubbleStyle이 할당되지 않았습니다.");
            return;
        }

        foreach (Transform child in holder)
        {
            child.gameObject.SetActive(false);
        }

        CreateRow(style.cornerTopLeft, style.edgeTop, style.cornerTopRight);

        for (int y = 1; y < height - 1; y++)
        {
            CreateRow(style.edgeLeft, style.centerTile, style.edgeRight, y == height - 2);
        }

        CreateRow(style.cornerBottomLeft, style.edgeBottom, style.cornerBottomRight);
    }

    private void CreateRow(SpriteTile left, SpriteTile middle, SpriteTile right, bool allowDecoration = false)
    {
        GameObject row = GetRow();
        row.transform.SetParent(holder);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
        {
            layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 0;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        GetTile(left).transform.SetParent(row.transform);

        int remainingDecorations = decorationCount;

        for (int i = 1; i < width - 1; i++)
        {
            if (allowDecoration && remainingDecorations > 0 && Random.value < 0.3f)
            {
                SpriteTile deco = Random.value < 0.5f ? style.decorationTileA : style.decorationTileB;
                GetTile(deco).transform.SetParent(row.transform);
                remainingDecorations--;
            }
            else
            {
                GetTile(middle).transform.SetParent(row.transform);
            }
        }

        GetTile(right).transform.SetParent(row.transform);
    }
}

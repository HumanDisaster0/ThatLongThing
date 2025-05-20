using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[System.Serializable]
public class SpriteTile
{
    public Sprite sprite;
    public Vector2 localScale = Vector2.one;
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
        newRow.transform.SetParent(holder, false);
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
        foreach (var tile1 in pooledTiles)
        {
            if (!tile1.activeSelf && tile1.name == tileData.sprite.name)
            {
                tile1.SetActive(true);
                return tile1;
            }
        }

        GameObject tile = new GameObject(tileData.sprite.name, typeof(RectTransform), typeof(Image));
        tile.transform.localScale = new Vector3(tileData.localScale.x, tileData.localScale.y, 1f);
        tile.transform.rotation = Quaternion.Euler(0, 0, tileData.rotationZ);

        Image image = tile.GetComponent<Image>();
        image.sprite = tileData.sprite;
        image.SetNativeSize();

        pooledTiles.Add(tile);
        return tile;
    }

    void Start()
    {
        BuildBubble();
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
            CreateRow(style.edgeLeft, style.centerTile, style.edgeRight);
        }
        CreateRow(style.cornerBottomLeft, style.edgeBottom, style.cornerBottomRight);
    }

    private void CreateRow(SpriteTile left, SpriteTile middle, SpriteTile right)
    {
        GameObject row = GetRow();
        row.transform.SetParent(holder, false);

        GetTile(left).transform.SetParent(row.transform, false);

        for (int i = 1; i < width - 1; i++)
        {
            GetTile(middle).transform.SetParent(row.transform, false);
        }

        GetTile(right).transform.SetParent(row.transform, false);
    }
}

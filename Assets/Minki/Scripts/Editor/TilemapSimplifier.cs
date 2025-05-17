using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSimplifier
{
    [MenuItem("Tools/Simplify Tilemap Colliders")]
    static void Simplify()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("Tilemap�� ���õ��� �ʾҽ��ϴ�.");
            return;
        }

        var tilemap = Selection.activeGameObject.GetComponent<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("������ ������Ʈ�� Tilemap ������Ʈ�� �����ϴ�.");
            return;
        }

        var bounds = tilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            int startX = bounds.xMin;
            while (startX < bounds.xMax)
            {
                if (tilemap.HasTile(new Vector3Int(startX, y, 0)))
                {
                    int endX = startX;
                    while (endX < bounds.xMax && tilemap.HasTile(new Vector3Int(endX, y, 0)))
                        endX++;

                    var bc = tilemap.gameObject.AddComponent<BoxCollider2D>();
                    bc.offset = tilemap.CellToLocalInterpolated(
                        new Vector3((startX + endX - 1) / 2f + 0.5f, y + 0.5f, 0));
                    bc.size = new Vector2(endX - startX, 1);
                    startX = endX;
                }
                else startX++;
            }
        }

        Debug.Log("Tilemap �浹 �ܼ�ȭ �Ϸ�.");
    }
}
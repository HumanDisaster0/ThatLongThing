using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.Tilemaps;

public class AutoCheckAbility : MonoBehaviour
{
    public GameObject AutoMappinPrefab;
    public RectTransform AutoMappinsParent;

    MagicAbility m_magicAbility;
    bool m_activated;

    Tilemap m_tilemap;
    RectTransform m_refRect;
    int m_pivotX;
    int m_pivotY;
    int m_maxY;

    Collider2D[] m_colliders = new Collider2D[8];

    HashSet<int> m_checkedTrapInfo = new HashSet<int>();

    // Start is called before the first frame update
    void Start()
    {
        m_magicAbility = GetComponent<MagicAbility>();
        m_magicAbility.OnStartMagic += ActivateAbility;
        m_magicAbility.OnEndMagic += DeactivateAbility;

        if(!AutoMappinsParent)
        {
            Transform foundChild = GameObject.FindWithTag("Minimap").GetComponentsInChildren<Transform>(true) // 비활성화된 자식 포함
                                        .FirstOrDefault(t => t.name == "AutoCheck");
            AutoMappinsParent = foundChild?.GetComponent<RectTransform>();
        }

        m_tilemap = GameObject.FindWithTag("Minimap").GetComponentsInChildren<Transform>(true) // 비활성화된 자식 포함
                                        .FirstOrDefault(t => t.name == "MiniMapTilemap").GetComponent<Tilemap>();

        m_refRect = GameObject.FindWithTag("Minimap").GetComponentsInChildren<Transform>(true) // 비활성화된 자식 포함
                                        .FirstOrDefault(t => t.name == "Scroll View").GetComponent<RectTransform>();

        ApplyTileInfo();

        MinimapTileInfo.OnChangedTileSize += ApplyTileInfo;
    }

    private void OnDestroy()
    {
        MinimapTileInfo.OnChangedTileSize -= ApplyTileInfo;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_activated)
            return;

        CheckAndSetMappin();
    }
    public void CheckAndSetMappin()
    {
        var overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, m_magicAbility.radius * m_magicAbility.magicLevel * Mathf.Sqrt(2f) * m_magicAbility.scale, m_colliders, m_magicAbility.FXLayer);

        if (overlapCount > 0)
        {
            for (int i = 0; i < overlapCount; i++)
            {
                if (m_colliders[i].tag != m_magicAbility.trapInfoTag
                    && m_colliders[i].tag != m_magicAbility.magicFXTag)
                    continue;

                if (!m_colliders[i].TryGetComponent(out AutoCheckTrapInfo autoCheckInfo))
                    continue;

                var trapinfo = autoCheckInfo.RefTrapInfo;

                if(m_checkedTrapInfo.Contains(trapinfo.GetHashCode()))
                {
                    continue; //이미 체크한 트랩은 무시
                }
                m_checkedTrapInfo.Add(trapinfo.GetHashCode());

                //트랩을 미니맵에 핀으로 표시
                var pinGO = Instantiate(AutoMappinPrefab, AutoMappinsParent);
                var pinRect = pinGO.GetComponent<RectTransform>();

                pinRect.anchoredPosition = new Vector2(
                    (trapinfo.transform.position.x * MinimapTileInfo.tileSize) + m_pivotX - (pinRect.sizeDelta.x * 0.5f),
                    (trapinfo.transform.position.y * MinimapTileInfo.tileSize) - (m_maxY * MinimapTileInfo.tileSize) - m_pivotY + (pinRect.sizeDelta.y * 0.5f)
                );

                pinGO.GetComponent<StaticMapPin>().MapPinState = ConvertTrapTypeToMapPinState(trapinfo.type);
            }
        }
    }

    public void ActivateAbility()
    {
        m_activated = true;
    }

    public void DeactivateAbility()
    {
        m_activated = false;
    }

    MapPinState ConvertTrapTypeToMapPinState(TrapType trapType)
    {
        switch(trapType)
        {
            case TrapType.Fine:
                return MapPinState.Fine;
            case TrapType.Danger:
                return MapPinState.Danger;
            case TrapType.Strange:
                return MapPinState.Strange;
            default:
                return MapPinState.Danger; //기본값
        }
    }

    void ApplyTileInfo()
    {
        //월드 좌표 계산을 위한 사전 정보 수집
        m_tilemap.CompressBounds();
        BoundsInt bounds = m_tilemap.cellBounds;
        int texWidth = bounds.size.x * MinimapTileInfo.tileSize;
        int texHeight = bounds.size.y * MinimapTileInfo.tileSize;

        int canvasWidth = (int)m_refRect.sizeDelta.x;
        int canvasHeight = (int)m_refRect.sizeDelta.y;

        m_pivotX = System.Math.Max(0, (canvasWidth - texWidth) / 2);
        m_pivotY = System.Math.Max(0, (canvasHeight - texHeight) / 2);

        m_maxY = m_tilemap.cellBounds.max.y;
    }
}

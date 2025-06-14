using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPinSetter : MonoBehaviour, IPointerDownHandler
{
    public static bool IsPinSetterActive { get; set; } = true;

    public const string COLOR_ERROR = "#FF0000";
    public const string COLOR_INFO = "#FFFFFF";

    public RectTransform mapContent; // 실제 지도
    public MapZoom zoom;
    public GameObject pinPrefab;
    public Text pinCountText;

    public bool UseTilemapScale = true;

    public IEnumerable<Transform> pins => m_pins.Values;
    public int maxPinCount = 5;

    Transform m_autoPinsParent;
   
    Dictionary<int, Transform> m_pins = new Dictionary<int, Transform>();
    List<int> m_deletePendingPins = new List<int>();

    private void Start()
    {
        m_autoPinsParent = GameObject.FindWithTag("Minimap").GetComponentsInChildren<Transform>(true) // 비활성화된 자식 포함
                            .FirstOrDefault(t => t.name == "AutoCheck");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(!IsPinSetterActive)
            return;

        if (eventData.pointerCurrentRaycast.gameObject == gameObject
           && eventData.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mapContent, Input.mousePosition, null, out var localPoint);

            if (m_pins.Count == maxPinCount)
                return;

            SoundManager.instance.PlayNewBackSound("Map_Check");

            var pinGO = Instantiate(pinPrefab);
            pinGO.transform.SetParent(mapContent, false);
            var pinRect = pinGO.GetComponent<RectTransform>();
            if(UseTilemapScale)
            {
                pinRect.sizeDelta = new Vector2(MinimapTileInfo.tileSize, MinimapTileInfo.tileSize);
            }
            pinRect.anchoredPosition = localPoint + new Vector2(-pinRect.sizeDelta.x * 0.5f * (1/zoom.GetScale) + 5.0f, pinRect.sizeDelta.y * 0.5f * (1 / zoom.GetScale) - 2.5f);
           
            pinGO.GetComponent<MapPin>().pinSetter = this;
            m_pins[pinGO.GetHashCode()] = pinGO.transform;
        }
    }

    private void Update()
    {
        if (m_deletePendingPins.Count > 0)
        {
            foreach (var hash in m_deletePendingPins)
            {
                if (m_pins.ContainsKey(hash))
                {
                    Destroy(m_pins[hash].gameObject);
                    m_pins.Remove(hash);
                }
            }
            m_deletePendingPins.Clear();
        }

        var trueScale = 1.0f / zoom.GetScale;

        foreach (var pair in m_pins)
        {
            pair.Value.localScale = new Vector3(trueScale, trueScale, 1);
        }

        if (m_autoPinsParent != null)
        {
            pinCountText.text = $"<color={(m_pins.Count + m_autoPinsParent.childCount > maxPinCount ? COLOR_ERROR : COLOR_INFO)}>{m_pins.Count + m_autoPinsParent.childCount} / {maxPinCount}</color>";
        }
        else
        {
            pinCountText.text = $"<color={COLOR_INFO}>{m_pins.Count} / {maxPinCount}</color>";
        }
    }

    public void DeletePin(int hash)
    {
        m_deletePendingPins.Add(hash);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapPinSetter : MonoBehaviour, IPointerClickHandler
{
    public RectTransform mapContent; // 실제 지도
    public MapZoom zoom;
    public GameObject pinPrefab;

    public int maxPinCount = 5;

    RectTransform m_rect;
    Dictionary<int, Transform> m_pins = new Dictionary<int, Transform>();
    List<int> m_deletePendingPins = new List<int>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerCurrentRaycast.gameObject == gameObject
           && eventData.button == PointerEventData.InputButton.Left)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mapContent, Input.mousePosition, null, out var localPoint);

            if (m_pins.Count == maxPinCount)
                return;

            var pinGO = Instantiate(pinPrefab);
            pinGO.transform.SetParent(mapContent, false);
            pinGO.GetComponent<RectTransform>().anchoredPosition = localPoint + new Vector2(-mapContent.sizeDelta.x * 0.5f, mapContent.sizeDelta.y * 0.5f);
            pinGO.GetComponent<MapPin>().pinSetter = this;
            m_pins[pinGO.GetHashCode()] = pinGO.transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(m_deletePendingPins.Count > 0)
        {
            print(m_deletePendingPins.Count);
            foreach(var hash in m_deletePendingPins)
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

        foreach(var pair in m_pins)
        {
            pair.Value.localScale = new Vector3(trueScale, trueScale, 1);
        }
    }

    public void DeletePin(int hash)
    {
        m_deletePendingPins.Add(hash);
    }
}

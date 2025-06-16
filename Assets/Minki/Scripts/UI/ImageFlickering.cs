using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFlickering : MonoBehaviour
{
    public float flickeringRate = 0.8f;

    float m_flickeringTimer = 0.0f;
    bool m_AlphaColIsZero = false;
    Image m_image;
    

    // Start is called before the first frame update
    void Start()
    {
        m_image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        m_flickeringTimer += Time.deltaTime;

        if (m_flickeringTimer > flickeringRate)
        {
            m_flickeringTimer -= flickeringRate;
            m_AlphaColIsZero = !m_AlphaColIsZero;
        }

        m_image.color = new Color(1.0f, 1.0f, 1.0f, m_AlphaColIsZero ? 0.0f : 1.0f);
    }
}

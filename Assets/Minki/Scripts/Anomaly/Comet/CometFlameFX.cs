using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CometFlameFX : MonoBehaviour
{
    public float xSeed = 0.45f;
    public float speed = 4f;
    SpriteRenderer m_sprite;
    Color m_defCol;

    // Start is called before the first frame update
    void Start()
    {
        m_sprite = GetComponent<SpriteRenderer>();
        m_defCol = m_sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        m_sprite.color = new Color(m_defCol.r, m_defCol.g, m_defCol.b, 1 - Mathf.PerlinNoise(xSeed,Time.time * speed) * 0.7f);
    }
}

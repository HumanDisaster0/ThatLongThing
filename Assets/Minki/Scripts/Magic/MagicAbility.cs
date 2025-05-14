using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagicAbility : MonoBehaviour
{
    [Header("Component")]
    public SpriteRenderer sprite;

    [Header("Ability Property")]
    public float radius = 6.0f;
    public float useTime = 5.0f;
    public float rotateSpeed = 20f;

    [Header("FX")]
    public string FXTag = "MagicFX";
    public LayerMask FXLayer = (1 << 5);
    public float MagicSpriteSize = 8.0f;

    Dictionary<int, MagicFX> m_foundFX = new Dictionary<int, MagicFX>();
    HashSet<int> m_currentFX = new HashSet<int>();
    List<int> m_keysToRemove = new List<int>();
    Collider2D[] m_colliders = new Collider2D[12];
    float m_useTimer = 0.0f;
    bool m_useAbility = false;
    bool m_firstFound;

    // Update is called once per frame
    void Update()
    {
        //키입력 확인
        if (!m_useAbility && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            m_useAbility = true;
            m_firstFound = true;
        }

        //능력 사용중일 때
        if(m_useAbility)
        {
            sprite.enabled = true;
            m_useTimer += Time.deltaTime;

            sprite.transform.localScale = Vector3.one * radius * MagicSpriteSize;
            sprite.transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));

            var overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, m_colliders,FXLayer);

            m_currentFX.Clear();
            m_keysToRemove.Clear();
            if (overlapCount > 0)
            {
                for(int i = 0; i < overlapCount; i++)
                {
                    if (m_colliders[i].tag != FXTag)
                        continue;

                    var fx = m_colliders[i].GetComponent<MagicFX>();
                    fx.SPR1.enabled = true;
                    fx.SPR2.enabled = true;

                    m_currentFX.Add(fx.GetHashCode());

                    if (!m_foundFX.ContainsKey(fx.GetHashCode()))
                    {
                        m_foundFX[fx.GetHashCode()] = fx;
                    }
                        

                }

                foreach (var pair in m_foundFX)
                {
                    int hash = pair.Key;
                    if (!m_currentFX.Contains(hash))
                    {
                        pair.Value.SPR1.enabled = false;
                        pair.Value.SPR2.enabled = false;
                        m_keysToRemove.Add(hash);
                    }
                }

                // 딕셔너리에서 제거
                foreach (var key in m_keysToRemove)
                {
                    m_foundFX.Remove(key);
                }

                m_firstFound = false;
            }
            else
            {
                foreach(var fx in m_foundFX.Values)
                {
                    fx.SPR1.enabled = false;
                    fx.SPR2.enabled = false;
                }
                m_foundFX.Clear();
            }

            if (m_useTimer > useTime)
            {
                m_useAbility = false;
                m_useTimer = 0.0f;
                sprite.enabled = false;
            }    
        }
        else
        {
            if (m_foundFX.Count > 0)
            {
                foreach (var fx in m_foundFX.Values)
                {
                    fx.SPR1.enabled = false;
                    fx.SPR2.enabled = false;
                }
                m_foundFX.Clear();
            }
        }
    }
}

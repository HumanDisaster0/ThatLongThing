using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagicAbility : MonoBehaviour
{
    [Header("Component")]
    public SpriteRenderer magicSprite;
    public SpriteRenderer BGSprite;

    [Header("Ability Property")]
    public float radius = 6.0f;
    public float useTime = 5.0f;
    public float rotateSpeed = 20f;

    [Header("FX")]
    public GameObject MagicFXPrefab;
    public string trapInfoTag = "TrapInfo";
    public float MagicSpriteSize = 8.0f;
    public int FXPoolCount = 10;


    public Color BGSpriteColor = new Color(1.0f, 1.0f, 1.0f, 0.4f);

    Dictionary<int, MagicFX> m_allocatedFX = new Dictionary<int, MagicFX>();
    HashSet<int> m_foundTrap = new HashSet<int>();
    HashSet<int> m_currentTrap = new HashSet<int>();
    List<int> m_keysToRemove = new List<int>();
    Collider2D[] m_colliders = new Collider2D[12];
    float m_useTimer = 0.0f;
    bool m_useAbility = false;
    Color m_t1Color = new Color(0, 0, 0, 0);
    Color m_t2Color = new Color(0, 0, 0, 0);
    float m_bgTimer;
    List<MagicFX> m_magicFXPool = new List<MagicFX>();

    private void Start()
    {
        if(m_magicFXPool.Count == 0)
        {
            for(int i = 0; i < FXPoolCount; i++)
            {
                var fx = Instantiate(MagicFXPrefab).GetComponent<MagicFX>();
                fx.enabled = false;
                fx.SPR1.enabled = false;
                fx.SPR2.enabled = false;
                m_magicFXPool.Add(fx);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //키입력 확인
        if (!m_useAbility && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            m_useAbility = true;
            m_t1Color = new Color(0,0,0,0);
            m_t2Color = BGSpriteColor;
            m_bgTimer = 0.0f;
        }

        m_bgTimer += Time.deltaTime;
        BGSprite.color = Color.Lerp(m_t1Color, m_t2Color, m_bgTimer / 0.25f);

        //능력 사용중일 때
        if (m_useAbility)
        {
            magicSprite.enabled = true;
            m_useTimer += Time.deltaTime;

            magicSprite.transform.localScale = Vector3.one * radius * MagicSpriteSize;
            magicSprite.transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));

            var overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius, m_colliders, -1);

            m_currentTrap.Clear();
            m_keysToRemove.Clear();
            if (overlapCount > 0)
            {
                for(int i = 0; i < overlapCount; i++)
                {
                    if (m_colliders[i].tag != trapInfoTag)
                        continue;

                    var trapinfo = m_colliders[i].GetComponent<TrapInfo>();
                    if (trapinfo?.type == TrapType.Fine)
                        continue;

                    m_currentTrap.Add(m_colliders[i].GetHashCode());

                    //fx활성화 - 풀에서 가져오기
                    if (!m_foundTrap.Contains(m_colliders[i].GetHashCode()))
                    {
                        m_foundTrap.Add(m_colliders[i].GetHashCode());

                        MagicFX fx;
                        if (m_magicFXPool.Count > 0)
                        {
                            fx = m_magicFXPool[m_magicFXPool.Count - 1];
                            m_magicFXPool.RemoveAt(m_magicFXPool.Count - 1);
                        }
                        else
                        {
                            fx = Instantiate(MagicFXPrefab).GetComponent<MagicFX>();
                        }
                        
                        fx.enabled = true;
                        fx.SPR1.enabled = true;
                        fx.SPR2.enabled = true;
                        fx.transform.position = m_colliders[i].transform.position;
                        fx.RestartAnimation();

                        m_allocatedFX.Add(m_colliders[i].GetHashCode(), fx);
                    }
                }

                foreach (var hash in m_foundTrap)
                {
                    if (!m_currentTrap.Contains(hash))
                    {
                        var value = m_allocatedFX[hash];

                        value.enabled = false;
                        value.SPR1.enabled = false;
                        value.SPR2.enabled = false;
                        m_magicFXPool.Add(value);
                        m_keysToRemove.Add(hash);
                    }
                }

                // 딕셔너리에서 제거
                foreach (var key in m_keysToRemove)
                {
                    m_foundTrap.Remove(key);
                    m_allocatedFX.Remove(key);
                }
            }
            else
            {
                foreach(var fx in m_allocatedFX.Values)
                {
                    fx.enabled = false;
                    fx.SPR1.enabled = false;
                    fx.SPR2.enabled = false;
                    m_magicFXPool.Add(fx);
                }
                m_foundTrap.Clear();
                m_allocatedFX.Clear();
            }

            if (m_useTimer > useTime)
            {
                m_useAbility = false;
                m_useTimer = 0.0f;
                magicSprite.enabled = false;
                m_t1Color = BGSpriteColor;
                m_t2Color = new Color(0, 0, 0, 0);
                m_bgTimer = 0.0f;
            }    
        }
        else
        {
            if (m_foundTrap.Count > 0) 
            {
                foreach (var fx in m_allocatedFX.Values)
                {
                    fx.enabled = false;
                    fx.SPR1.enabled = false;
                    fx.SPR2.enabled = false;
                    m_magicFXPool.Add(fx);
                }
                m_foundTrap.Clear();
                m_allocatedFX.Clear();
            }

        }
    }
}


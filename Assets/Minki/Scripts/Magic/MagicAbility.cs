using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MagicAbility : MonoBehaviour
{
    [Header("Component")]
    public SpriteRenderer magicSpriteRender;
    public SpriteRenderer BGSpriteRender;
    public Sprite lowLevelSpr;
    public Sprite MidLevelSpr;
    public Sprite HighLevelSpr;

    [Header("Ability Property")]
    public float radius = 0.5f;
    public float useTime = 5.0f;
    public float rotateSpeed = 20f;
    [Range(1,3)]
    public int magicLevel = 1;

    [Header("FX")]
    public GameObject MagicFXPrefab;
    public string trapInfoTag = "TrapInfo";
    public string magicFXTag = "MagicFX";
    public float MagicSpriteSize = 8.0f;
    public int FXPoolCount = 10;
    public LayerMask FXLayer = (1 << 5);


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
        magicLevel = Mathf.Clamp(magicLevel, 1, 3);

        switch(magicLevel)
        {
            case 1:
                magicSpriteRender.sprite = lowLevelSpr;
                break;
            case 2:
                magicSpriteRender.sprite = MidLevelSpr;
                break;
            case 3:
                magicSpriteRender.sprite = HighLevelSpr;
                break;
        }

        //키입력 확인
        if (!m_useAbility && Input.GetKeyDown(KeyCode.LeftAlt))
        {
            SoundManager.instance.PlayNewSound("Magic_Execute", gameObject);
            m_useAbility = true;
            m_t1Color = new Color(0,0,0,0);
            m_t2Color = BGSpriteColor;
            m_bgTimer = 0.0f;
        }

        m_bgTimer += Time.deltaTime;
        BGSpriteRender.color = Color.Lerp(m_t1Color, m_t2Color, m_bgTimer / 0.25f);

        //능력 사용중일 때
        if (m_useAbility)
        {
            magicSpriteRender.enabled = true;
            m_useTimer += Time.deltaTime;

            magicSpriteRender.transform.Rotate(new Vector3(0, 0, rotateSpeed * Time.deltaTime));

            var overlapCount = Physics2D.OverlapCircleNonAlloc(transform.position, radius * magicLevel * Mathf.Sqrt(2f), m_colliders, FXLayer);

            m_currentTrap.Clear();
            m_keysToRemove.Clear();
            if (overlapCount > 0)
            {
                for(int i = 0; i < overlapCount; i++)
                {
                    if (m_colliders[i].tag != trapInfoTag 
                        && m_colliders[i].tag != magicFXTag)
                        continue;

                    var trapinfo = m_colliders[i].GetComponent<TrapInfo>();
                    if (trapinfo && (trapinfo?.type == TrapType.Fine || trapinfo.dontShowFX))
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
                        fx.transform.parent = m_colliders[i].transform;
                        fx.transform.position = m_colliders[i].transform.position;
                        fx.RestartAnimation();
                        fx.SetLevel(magicLevel);
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
                        value.transform.parent = null;
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
                    fx.transform.parent = null;
                    m_magicFXPool.Add(fx);
                }
                m_foundTrap.Clear();
                m_allocatedFX.Clear();
            }

            if (m_useTimer > useTime)
            {
                m_useAbility = false;
                m_useTimer = 0.0f;
                magicSpriteRender.enabled = false;
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
                    fx.transform.parent = null;
                    m_magicFXPool.Add(fx);
                }
                m_foundTrap.Clear();
                m_allocatedFX.Clear();
            }

        }
    }
}


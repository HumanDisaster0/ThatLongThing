using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagicHUD : MonoBehaviour
{
    public Image image;
    public Sprite ActiveSpr;
    public Sprite DeactiveSpr;

    MagicAbility m_ability;

    // Start is called before the first frame update
    void Start()
    {
        m_ability = GameObject.FindWithTag("Player").GetComponentInChildren<MagicAbility>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_ability.IsUsedAbility)
        {
            image.sprite = ActiveSpr;
        }
        else
        {
            image.sprite = DeactiveSpr;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorAnomalyBGMTrigger : MonoBehaviour
{
    bool m_isTriggered;
    Rigidbody2D m_rb;

    // Start is called before the first frame update
    void Start()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!m_isTriggered && collision.CompareTag("Player"))
        {
            //중복 방지
            m_isTriggered = true;

            //BGM변경 시작!!
            StartCoroutine(ChangeBGM());
        }
    }

    public void ResetTrigger()
    {
        m_isTriggered = false;
        StopAllCoroutines();
    }


    //BGM변경용 코루틴
    IEnumerator ChangeBGM()
    {
        yield return null;
    }
}

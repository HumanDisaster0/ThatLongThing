using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDontJump : MonoBehaviour
{
    // Start is called before the first frame update
    PlayerController m_pc;
    void Start()
    {
        m_pc = GetComponent<PlayerController>();
        m_pc.DontJump = true;
    }
}

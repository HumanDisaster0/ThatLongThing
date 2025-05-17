using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace Minki
{
    public class RotatePlatform : MonoBehaviour
    {
        public float rotateAngle = 30.0f;

        TilemapCollider2D m_col;
        Rigidbody2D m_rb;

        bool m_isFreeze = false;
        bool m_trapOff;

        private void Awake()
        {
            m_col = GetComponent<TilemapCollider2D>();
            m_rb = GetComponent<Rigidbody2D>();

            m_rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            m_rb.bodyType = RigidbodyType2D.Kinematic;
            m_rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void FixedUpdate()
        {
            if (!m_isFreeze 
                && !m_trapOff)
            {
                m_rb.angularVelocity = rotateAngle;
            }
        }

        public void SetFreeze(bool value) { m_isFreeze = value; }

        public void ResetPlatform()
        {
            m_rb.rotation = 0.0f;
        }

        public void TrapOn()
        {
            if (!gameObject.activeInHierarchy)
                return;

            m_trapOff = false;
        }

        public void TrapOff()
        {
            if (!gameObject.activeInHierarchy)
                return;

            m_trapOff = true;
        }
    }

}
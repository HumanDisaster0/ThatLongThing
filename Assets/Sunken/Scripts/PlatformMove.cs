using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMove : MonoBehaviour
{
    [SerializeField] List<Transform> wayPoints;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] bool loop = true;
    [SerializeField] bool move = false;

    [SerializeField] private int currIdx = 0;
    private bool isMoving = false;
    private bool isReversal = false;

    // Start is called before the first frame update
    void Start()
    {
        if(wayPoints.Count == 0)
        {
            wayPoints.Add(transform);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (move)
        {
            CycleWayPoints();
        }
    }

    private void CycleWayPoints()
    {
        if (CloseEnough(wayPoints[currIdx]))
        {
            if (loop)
            {
                if (currIdx == wayPoints.Count - 1)
                    isReversal = true;
                if (currIdx == 0)
                    isReversal = false;

                int shift = isReversal ? -1 : 1;
                currIdx += shift;
            }
            else
            {
                if (currIdx == wayPoints.Count - 1)
                    move = false;
                else
                    currIdx++;
            }
        }

        MovePlatform(wayPoints[currIdx]);
    }

    private void MovePlatform(Transform target)
    {
        Vector2 moveDir = (target.position - transform.position).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = moveDir * moveSpeed;
    }

    private bool CloseEnough(Transform target)
    {
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            transform.position = target.position;
            return true;
        }
        else
            return false;
    }
}

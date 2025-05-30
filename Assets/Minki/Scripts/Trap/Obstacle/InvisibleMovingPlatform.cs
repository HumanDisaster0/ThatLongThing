using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InvisibleMovingPlatform : MonoBehaviour
{
    enum PlatformStatus
    {
        stop = 0,
        move
    }

    [SerializeField] List<Transform> wayPoints;
    [SerializeField] float moveSpeed = 1.0f;
    [SerializeField] float moveSharpness = 8.0f;
    [SerializeField] float waitTime = 1.0f;
    [SerializeField] bool loop = true;
    [SerializeField] PlatformStatus currStat = PlatformStatus.move;
    [SerializeField] Tilemap tilemap;
    

    Rigidbody2D m_rb;

    private Vector2 initPos = Vector2.zero;
    private int currIdx = 0;
    private bool isReversal = false;
    private PlatformStatus resetStat;

    // Start is called before the first frame update
    void Start()
    {
        if (wayPoints.Count == 0)
        {
            wayPoints.Add(transform);
        }
        m_rb = GetComponent<Rigidbody2D>();
        initPos = transform.position;
    }

    private void Update()
    {
        if(Mathf.Abs(m_rb.velocity.y) > 0.05f)
        {
            tilemap.color = Color.Lerp(tilemap.color, new Color(1, 1, 1, 0), 12f * Time.deltaTime);
        }
        else
        {
            tilemap.color = Color.Lerp(tilemap.color, Color.white, 12f * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currStat == PlatformStatus.move)
        {
            CycleWayPoints();
        }
    }

    private void CycleWayPoints()
    {
        if (CloseEnough(wayPoints[currIdx]))
        {
            currStat = PlatformStatus.stop;
            StartCoroutine(Waiting(waitTime));
        }

        MovePlatform(wayPoints[currIdx]);
    }

    private void MovePlatform(Transform target)
    {
        Vector2 moveDir = (target.position - transform.position).normalized;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        switch (currStat)
        {
            case PlatformStatus.move:
                float distance = Vector2.Distance(transform.position, target.position);
                float adjustedSpeed = Mathf.Lerp(0, moveSpeed, distance / 1.0f); // 거리 기반 속도 조절
                rb.velocity = Vector2.Lerp(rb.velocity, moveDir * adjustedSpeed, moveSharpness * Time.deltaTime);
                break;
            case PlatformStatus.stop:
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, moveSharpness * Time.deltaTime);
                break;
            default:
                currStat = PlatformStatus.stop;
                break;
        }
    }

    public void SetCurrStat(int statIndex)
    {
        currStat = (PlatformStatus)statIndex;
        resetStat = (PlatformStatus)statIndex;
    }

    public void ResetPos()
    {
        StopAllCoroutines();
        currIdx = 0;
        transform.position = initPos;
        currStat = resetStat;
    }

    private bool CloseEnough(Transform target)
    {
        if (Vector2.Distance(transform.position, target.position) < 0.01f)
        {
            transform.position = target.position;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            currStat = PlatformStatus.stop;
            return true;
        }
        else
            return false;
    }


    IEnumerator Waiting(float time)
    {
        yield return new WaitForSeconds(time);

        if (loop)
        {
            if (currIdx == wayPoints.Count - 1)
                isReversal = true;
            else if (currIdx == 0)
                isReversal = false;

            int shift = isReversal ? -1 : 1;
            currIdx += shift;
        }
        else
        {
            if (currIdx == wayPoints.Count - 1)
                currStat = PlatformStatus.stop;
            else
                currIdx++;
        }

        currStat = PlatformStatus.move;
    }

    private void OnDrawGizmosSelected()
    {
        float radius = 0.1f;
        Gizmos.color = Color.red;
        foreach (Transform t in wayPoints)
            Gizmos.DrawWireSphere(t.position, radius);

        for (int i = 0; i < wayPoints.Count - 1; i++)
            Gizmos.DrawLine(wayPoints[i].position, wayPoints[i + 1].position);
    }
}

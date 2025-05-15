using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRange : MonoBehaviour
{
    [Header("움직임 범위설정")]
    public float minRangeX, maxRangeX;

    private float initMinX, initMaxX;

    private void Start()
    {
        initMinX = minRangeX;
        initMaxX = maxRangeX;
    }

    public void InitRange()
    {
        minRangeX = initMinX;
        maxRangeX = initMaxX;
    }

    public float GetRandomPosX()
    {
        float pickpos = transform.position.x + Random.Range(minRangeX, maxRangeX);

        return pickpos;
    }

    public float GetMinX()
    {
        return transform.position.x + minRangeX;
    }

    public float GetMaxX()
    {
        return transform.position.x + maxRangeX;
    }

    public void SetRange(Vector2 _pos, GameObject _obj)
    {
        if (_pos.x > transform.position.x + minRangeX && _pos.x < transform.position.x + maxRangeX)
        {
            if(_obj.transform.position.x < _pos.x)
                maxRangeX = _obj.transform.position.x - transform.position.x;
            else if (_obj.transform.position.x > _pos.x)
                minRangeX = _obj.transform.position.x - transform.position.x;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 minPos = transform.position + new Vector3(minRangeX,0,0);
        Vector3 maxPos = transform.position + new Vector3(maxRangeX,0,0);

        Gizmos.DrawWireSphere(minPos, 0.1f);
        Gizmos.DrawWireSphere(maxPos, 0.1f);
        Gizmos.DrawLine(minPos, maxPos);
    }
}

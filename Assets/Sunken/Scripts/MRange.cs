using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MRange : MonoBehaviour
{
    [Header("움직임 범위설정")]
    public float minRangeX, maxRangeX;

    public float GetRandomPosX()
    {
        float pickpos = transform.position.x + Random.Range(minRangeX, maxRangeX);

        return pickpos;
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

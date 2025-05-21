using System.Collections.Generic;
using UnityEngine;

public class RollingStoneMove : MonoBehaviour
{
    public CameraController cam;

    [Header("이동 속도 및 방향")]
    public Vector2 moveDirection = Vector2.right;
    public float moveSpeed = 2f;

    [Header("회전 연출")]
    public float rotationMultiplier = 360f;

    [Header("화면 흔들림 연출 간격")]
    public float shakeInterval = 0.7f;

    private float shakeTimer = 0f;

    private float nextShakeTime = 2f;
    private float irregularShakeTimer = 0f;

    private bool isActive = false;
    private Vector3 initialPosition;
    private Destroyer dest;

    private List<GameObject> m_destroyedEnemy = new List<GameObject>();

    void Start()
    {
        initialPosition = transform.position;  // 초기 위치 저장
        gameObject.SetActive(false);           // 시작 시 비활성화
        dest = GetComponent<Destroyer>();
    }

    void Update()
    {
        if (!isActive) return;

        // 이동
        Vector3 deltaMove = moveDirection.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(deltaMove, Space.World);

        // 이동 방향에 따라 회전
        float rotationAmount = rotationMultiplier * Time.deltaTime;
        float directionSign = Mathf.Sign(moveDirection.x);
        transform.Rotate(Vector3.forward, -rotationAmount * directionSign);

        shakeTimer += Time.deltaTime;
        if (shakeTimer >= shakeInterval)
        {
            cam?.ShakeCamera(35f, 0.18f, 6f);
            shakeTimer = 0f;
        }

        irregularShakeTimer += Time.deltaTime;
        if (irregularShakeTimer >= nextShakeTime)
        {
            SoundManager.instance?.PlayNewBackSound("Trex_Land");

            cam?.ShakeCamera(50f, 0.5f, 0.8f); // 강한 흔들림 한 번            
            irregularShakeTimer = 0f;
            nextShakeTime = Random.Range(2f, 5f); // 다음 타이밍 설정
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        int layer = other.gameObject.layer;

        if (layer == LayerMask.NameToLayer("Enemy"))
        {
            var move = other.GetComponent<MMove>();
            var sr = other.GetComponentInChildren<SpriteRenderer>();

            if (move != null && sr != null)
            {
                dest.SpawnFlyingEnemy(other.transform.position, sr.sprite);
                m_destroyedEnemy.Add(other.gameObject);
                other.gameObject.SetActive(false);
            }


        }
        else if (layer == LayerMask.NameToLayer("Player"))
        {
            var pc = other.GetComponent<PlayerController>();

            if (pc != null)
                pc.AnyState(PlayerState.Die);
            // else
            //       Debug.LogWarning($"[RollingStone] Player 오브젝트에 PlayerController 없음: {other.name}");
        }
    }

    public void RespawnDestroyedEnemy()
    {
        foreach (var go in m_destroyedEnemy)
            go.SetActive(true);

        m_destroyedEnemy.Clear();
    }


    // 외부에서 활성화 (등장)
    public void Activate()
    {
        transform.position = initialPosition; // 혹시 다른 곳에 있었더라도 복귀
        gameObject.SetActive(true);
        isActive = true;

        cam?.ShakeCamera(100f, 1f, 0.01f);

        irregularShakeTimer = 0f;
        nextShakeTime = Random.Range(3f, 5f); // 다음 타이밍 설정
    }
    // 외부에서 비활성화 (사라짐 + 위치 복구)
    public void Deactivate()
    {
        isActive = false;
        transform.position = initialPosition;
        gameObject.SetActive(false);
    }
}

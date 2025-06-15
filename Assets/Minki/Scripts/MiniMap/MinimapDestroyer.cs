using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class MinimapDestroyer : MonoBehaviour
{
    public Sprite[] FXImgs;
    public RectTransform ffanggoRect;
    public RectTransform minimapRect;
    public bool IsDestroied => m_isDestroied;

    MapOnOffControl m_mapOnOffControl;
    System.Random m_rand = new System.Random();
    bool m_isDestroied = false;

    Vector2 m_shakePos = Vector2.zero;
    float m_shakeAmount = 1.0f;
    int m_noiseSeed = 45;
    float m_shakeSpeed = 1.0f;
    float m_shakeRecovery = 1.0f;
    float m_shakeTrauma = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_mapOnOffControl = FindFirstObjectByType<MapOnOffControl>(FindObjectsInactive.Include);
    }

    public void StartDestroyMap(float delayTime)
    {
        StartCoroutine(DestroyMinimap(delayTime));
    }

    public void StartDestroyMap()
    {
        StartCoroutine(DestroyMinimap(0.0f));
    }

    public void StartDestroyMap(GameObject owner)
    {
        StartCoroutine(DestroyMinimap(0.0f));
    }

    private void Update()
    {
        ShakeCanvas();
    }

    IEnumerator DestroyMinimap(float delayTime)
    {
        //중복 방지
        if (m_isDestroied)
            yield break;

        m_isDestroied = true;

        var timer = 0.0f;
        while(timer < delayTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        //ESC 메뉴 강제 종료 - 감히 시간을 거스르려고 해? 바로 컷!
        if (PauseManager.Instance != null 
            && GuildRoomManager.Instance != null)
        {
            PauseManager.Instance.Resume();
            GuildRoomManager.Instance.isPauseAble = false;
        }

        //미니맵 켜!
        m_mapOnOffControl.activeControl = true;
        m_mapOnOffControl.ShowMinimap();

        //터치방지 켜기
        ffanggoRect.gameObject.SetActive(true);

        //강제로 기다려
        var menuEventTimer = 0.0f;
        while (menuEventTimer < 0.5f)
        {
            //연출이 끝나기 전까지 다른 곳에서 시간을 못 건드리게 할거임 - 토키요 토마레!! 
            Time.timeScale = 0.0f;
            menuEventTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        //미니맵 강제로 못끄게 할거임
        m_mapOnOffControl.activeControl = false;

        //빵꾸 갯수
        var ffanggooCount = FXImgs.Length;

        //빵꾸 이미지 배열 섞기
        int n = FXImgs.Length;

        for (int i = n - 1; i > 0; i--)
        {
            int j = m_rand.Next(i + 1); // 0부터 i까지의 무작위 인덱스
            (FXImgs[i], FXImgs[j]) = (FXImgs[j], FXImgs[i]); // 요소 스왑
        }

        //연출이 끝나기 전까지 다른 곳에서 시간을 못 건드리게 할거임 - 토키요 토마레!! 
        Time.timeScale = 0.0f;

        while (ffanggooCount > 0)
        {
            //찢을 꺼임 - 헉!
            var ffanggooGO = new GameObject($"FFANGGOO[{FXImgs.Length - ffanggooCount}]");
            var ffanggooRect = ffanggooGO.AddComponent<RectTransform>();
            var ffanggooImg = ffanggooGO.AddComponent<Image>();

            ffanggooRect.SetParent(ffanggoRect);

            //이미지 돌려쓰기
            ffanggooRect.anchorMin = Vector2.zero;
            ffanggooRect.anchorMax = Vector2.zero;
            ffanggooImg.sprite = FXImgs[FXImgs.Length - ffanggooCount];
            ffanggooImg.SetNativeSize();

            ffanggooRect.anchoredPosition = new Vector2(UnityEngine.Random.Range(0, ffanggoRect.sizeDelta.x), UnityEngine.Random.Range(0, ffanggoRect.sizeDelta.y));

            //소리내기
            SoundManager.instance?.PlayNewBackSound("Album_Click");

            //빵꾸 하나씩 딜레이 걸기용
            var ffanggooTimer = 0.0f;

            ShakeForceApply(24, 16.0f, 12.0f);

            while (ffanggooTimer < 0.12f)
            {
                //연출이 끝나기 전까지 다른 곳에서 시간을 못 건드리게 할거임 - 토키요 토마레!! 
                Time.timeScale = 0.0f;
                ffanggooTimer += Time.unscaledDeltaTime;

                minimapRect.anchoredPosition = m_shakePos * m_shakeTrauma;

                yield return null;
            }

            ffanggooCount--;
        }


        yield return null;


        timer = 0.0f;

        while (timer < 1.5f)
        {
            //연출이 끝나기 전까지 다른 곳에서 시간을 못 건드리게 할거임 - 토키요 토마레!! 
            Time.timeScale = 0.0f;
            timer += Time.unscaledDeltaTime;
            minimapRect.anchoredPosition = m_shakePos * m_shakeTrauma;
            yield return null;
        }


        ////미니맵 제어 활성화 및 강제로 끄기
        m_mapOnOffControl.activeControl = true;
        m_mapOnOffControl.HideMinimap();

        //시간은 움직인다.
        Time.timeScale = 1.0f;

        if (GuildRoomManager.Instance != null)
        {
            GuildRoomManager.Instance.isPauseAble = true;
        }

        minimapRect.anchoredPosition = Vector2.zero;
        m_shakePos = Vector2.zero;
        ShakeForceApply(0, 0.0f, 0.0f);
    }

    void ShakeCanvas()
    {
        m_shakePos = new Vector3(m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed, Time.unscaledTime * m_shakeSpeed) * 2 - 1),
        m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed + 1, Time.unscaledTime * m_shakeSpeed) * 2 - 1),
        m_shakeAmount * (Mathf.PerlinNoise(m_noiseSeed + 2, Time.unscaledTime * m_shakeSpeed) * 2 - 1));
        m_shakeTrauma = Mathf.Lerp(m_shakeTrauma, 0, m_shakeRecovery * Time.unscaledDeltaTime);
    }

    public void ShakeForceApply(float speed, float amount, float recovery)
    {
        m_shakeSpeed = speed;
        m_shakeAmount = amount;
        m_shakeRecovery = recovery;
        m_shakeTrauma = 1f;
    }
}

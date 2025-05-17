using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    [SerializeField] int sourceSize = 10;
    [SerializeField] float volumeGradation = 10f;
    [SerializeField] List<AudioSource> sources;
    [SerializeField] List<AudioSource> allSources;

    Camera cam;

    private void OnValidate()
    {
        while (sources.Count < sourceSize)
        {
            GameObject loaded = Resources.Load<GameObject>("Sound/DefaultSource");
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(loaded);
            obj.transform.SetParent(transform, false);
            sources.Add(obj.GetComponent<AudioSource>());
        }
        sources = GetComponentsInChildren<AudioSource>().ToList();
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        cam = Camera.main;

        allSources = GetComponentsInChildren<AudioSource>().ToList();
        LoadAllSound();
    }

    // Update is called once per frame
    void Update()
    {
        if(sources.Count != transform.childCount)
        {
            sources = GetComponentsInChildren<AudioSource>().ToList();
        }

        CheckVolume();
    }

    /// <summary>
    /// 중복 상관없이 재생
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlayNewSound(string soundName, GameObject caller)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            if (sources.Count != 0)
            {
                sources[sources.Count-1].clip = _clip;
                sources[sources.Count - 1].gameObject.transform.position =
                    new Vector3(caller.transform.position.x, caller.transform.position.y, sources[sources.Count - 1].transform.position.z);
                sources[sources.Count - 1].loop = false;
                sources[sources.Count - 1].Play();
                sources[sources.Count - 1].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[sources.Count - 1].gameObject));
                audioSource = sources[sources.Count-1];
                sources[sources.Count - 1].gameObject.transform.SetParent(caller.transform);
            }
            else
                Debug.LogWarning("사운드 매니저의 스피커 부족!!");
        }
        else
            Debug.LogError(caller + " 이 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    /// <summary>
    /// 중복없이 재생
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlaySound(string soundName, GameObject caller)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            // 자식 오브젝트에서 모든 AudioSource 가져오기
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // 현재 오디오가 soundName과 같은 경우
                {
                    isExist = true;
                    break;
                }
            }

            if (!isExist)
            {
                if (sources.Count != 0)
                {
                    sources[sources.Count - 1].clip = _clip;
                    sources[sources.Count - 1].gameObject.transform.position =
                        new Vector3(caller.transform.position.x, caller.transform.position.y, sources[sources.Count-1].transform.position.z);
                    sources[sources.Count - 1].loop = false;
                    sources[sources.Count - 1].Play();
                    sources[sources.Count - 1].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[sources.Count-1].gameObject));
                    audioSource = sources[sources.Count - 1];
                    sources[sources.Count - 1].gameObject.transform.SetParent(caller.transform);
                }
                else
                    Debug.LogWarning("사운드 매니저의 스피커 부족!!");
            }
        }
        else
            Debug.LogError(caller + " 이 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    public void StopSound(string soundName, GameObject caller)
    {
        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있는 경우 실행
        {
            // 자식 오브젝트에서 모든 AudioSource 가져오기
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            GameObject obj = null;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // 현재 오디오가 soundName과 같은 경우
                {
                    source.Stop(); // 사운드 정지
                    obj = source.gameObject;
                    break;
                }
            }

            if (obj)
            {
                Coroutine cor = obj.GetComponent<DefaultSourceData>().myCoroutine;
                obj.transform.SetParent(transform);
                StopCoroutine(cor);
                cor = null;
            }
        }
        else
            Debug.LogError(caller + " 이 잘못된 사운드를 정지하였음!!");
    }

    IEnumerator StopTime(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
        obj.transform.SetParent(transform);
    }

    private void LoadAllSound()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sound"); // "Resources/Sound" 폴더 내의 모든 클립을 로드

        foreach (AudioClip clip in clips)
        {
            if (!soundClips.ContainsKey(clip.name)) // 중복 방지
            {
                soundClips.Add(clip.name, clip);
            }
        }

        Debug.Log("총 " + soundClips.Count + "개의 사운드 로드 완료!");
    }

    private void CheckVolume()
    {
        //Vector2 leftDown = cam.ScreenToWorldPoint(new Vector2(0, 0));
        //Vector2 rightUp = cam.ScreenToWorldPoint(new Vector2(cam.scaledPixelWidth, cam.scaledPixelHeight));

        //foreach (var source in allSources)
        //{
        //    Vector2 pos = source.transform.position;

        //    // 오브젝트가 화면 경계 내에 있는지 확인
        //    bool isInside = (pos.x >= leftDown.x && pos.x <= rightUp.x) &&
        //                    (pos.y >= leftDown.y && pos.y <= rightUp.y);

        //    if (isInside)
        //    {
        //        source.volume = 1f; // 화면 안에 있을 때 볼륨 최대로 유지
        //    }
        //    else
        //    {
        //        // 화면 경계에서 얼마나 멀어졌는지 계산
        //        float distance = Mathf.Min(Vector2.Distance(pos, leftDown), Vector2.Distance(pos, rightUp));

        //        // 거리가 멀어질수록 볼륨 감소 (최소값 0 ~ 최대값 1)
        //        source.volume = Mathf.Clamp01(1f - (distance / volumeGradation)); // `10f`는 조절 가능한 감쇠 거리
        //    }
        //}

        foreach (var source in allSources)
        {
            Vector2 pos = source.transform.position;

            // 거리 기반으로 볼륨 조절
            float distanceFromCenter = Vector2.Distance(pos, cam.transform.position);
            float maxDistance = 10f;

            source.volume = Mathf.Clamp01(1f - (distanceFromCenter / maxDistance));
        }
    }
}

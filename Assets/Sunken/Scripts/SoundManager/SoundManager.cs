using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public bool isLowResource = true;

    [SerializeField] Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();
    [SerializeField] int sourceSize = 10;
    [SerializeField] List<AudioSource> sources;
    [SerializeField] List<AudioSource> activeSources;

    [Header("볼륨 조절")]
    [Range(0f,1f)]
    public float seVol = 1f; // SE
    [Range(0f, 1f)]
    public float bgVol = 1f; // BGM

    Camera cam;
    bool isAllStop = false;

#if UNITY_EDITOR
    //private void OnValidate()
    //{
    //    while (sources.Count < sourceSize)
    //    {
    //        GameObject loaded = Resources.Load<GameObject>("Sound/Prefab/DefaultSource");
    //        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(loaded);
    //        obj.transform.SetParent(transform);
    //        sources.Add(obj.GetComponent<AudioSource>());
    //    }
    //}
#endif

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        LoadAllSound();

        while (sources.Count < sourceSize)
        {
            GameObject loaded = Resources.Load<GameObject>("Sound/Prefab/DefaultSource");
            GameObject obj = Instantiate(loaded);
            obj.transform.SetParent(transform);
            sources.Add(obj.GetComponent<AudioSource>());
        }
        cam = Camera.main;
        sources = GetComponentsInChildren<AudioSource>().ToList();
    }

    private void Start()
    {
        bgVol = PlayerPrefs.GetFloat("bgVol", 1.0f);
        seVol = PlayerPrefs.GetFloat("seVol", 1.0f);
        isLowResource = PlayerPrefs.GetInt("isLowRes", 1) == 0 ? false : true;

        BgmPlayer.instance?.UpdateVolume();
    }

    // Update is called once per frame
    void Update()
    {
        CheckVolume();
    }

    //private void OnLevelWasLoaded(int level)
    //{
    //    cam = Camera.main;
        
    //}

    /// <summary>
    /// 중복 상관없이 재생
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlayNewSound(string soundName, GameObject caller, Vector2 offset = default, float maxDistance = 15f)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].gameObject.transform.position =
                    new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                sources[0].loop = false;
                sources[0].volume = seVol;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                audioSource = sources[0];

                sources[0].gameObject.transform.SetParent(caller.transform);
                sources[0].transform.position += new Vector3(offset.x, offset.y, 0);

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("사운드 매니저의 스피커 부족!!");
        }
        else
            Debug.LogWarning(caller + " 이 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    public AudioSource PlayNewBackSound(string soundName, SoundType type = SoundType.No)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].loop = false;
                sources[0].volume = seVol;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = false;
                audioSource = sources[0];

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("사운드 매니저의 스피커 부족!!");
        }
        else
            Debug.LogWarning("전역사운드가 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    public void PlayNewBtnSound(string soundName)
    {

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].loop = false;
                sources[0].volume = seVol;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = false;

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("사운드 매니저의 스피커 부족!!");
        }
        else
            Debug.LogWarning("전역사운드가 잘못된 사운드를 요청하였음!!");
    }

    /// <summary>
    /// 중복없이 재생
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlaySound(string soundName, GameObject caller, Vector2 offset = default, float maxDistance = 15f)
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
                    audioSource = source;
                    break;
                }
            }

            if (!isExist)
            {
                if (sources.Count != 0)
                {
                    sources[0].clip = _clip;
                    sources[0].gameObject.transform.position =
                        new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                    sources[0].loop = false;
                    sources[0].volume = seVol;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);
                    sources[0].transform.position += new Vector3(offset.x, offset.y, 0);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("사운드 매니저의 스피커 부족!!");
            }
        }
        else
            Debug.LogWarning(caller + " 이 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    public AudioSource PlayBackSound(string soundName)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있을경우 실행
        {
            bool isExist = false;

            foreach (AudioSource source in activeSources)
            {
                if (!source.GetComponent<DefaultSourceData>().isVolCon && source.clip == _clip) // 현재 오디오가 soundName과 같은 경우
                {
                    isExist = true;
                    audioSource = source;
                    break;
                }
            }

            if (!isExist)
            {
                if (sources.Count != 0)
                {
                    sources[0].clip = _clip;
                    sources[0].loop = false;
                    sources[0].volume = seVol;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = false;
                    audioSource = sources[0];

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("사운드 매니저의 스피커 부족!!");
            }
        }
        else
            Debug.LogWarning("전역사운드가 잘못된 사운드를 요청하였음!! : " + soundName);

        return audioSource;
    }

    public AudioSource PlayLoopSound(string soundName, GameObject caller, Vector2 offset = default, float maxDistance = 15f)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있는 경우 실행
        {
            // 자식 오브젝트에서 모든 AudioSource 가져오기
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // 현재 오디오가 soundName과 같은 경우
                {
                    isExist = true;
                    audioSource = source;
                    break;
                }
            }

            if (!isExist)
            {
                if (sources.Count != 0)
                {
                    sources[0].clip = _clip;
                    sources[0].gameObject.transform.position =
                        new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                    sources[0].loop = true;
                    sources[0].volume = seVol;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);
                    sources[0].transform.position += new Vector3(offset.x, offset.y, 0);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("사운드 매니저의 스피커 부족!!");
            }
        }
        else
            Debug.LogWarning(caller + " 이 잘못된 사운드를 요청하였음!!");

        return audioSource;
    }

    public AudioSource PlayLoopBackSound(string soundName, SoundType type = SoundType.No)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // 사운드가 있는 경우 실행
        {
            bool isExist = false;

            foreach (AudioSource source in activeSources)
            {
                if (!source.GetComponent<DefaultSourceData>().isVolCon && source.clip == _clip) // 현재 오디오가 soundName과 같은 경우
                {
                    isExist = true;
                    audioSource = source;
                    break;
                }
            }

            if (!isExist)
            {
                if (sources.Count != 0)
                {
                    sources[0].clip = _clip;
                    sources[0].loop = true;
                    sources[0].volume = seVol;
                    sources[0].Play();
                    sources[0].GetComponent<DefaultSourceData>().isVolCon = false;
                    audioSource = sources[0];

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("사운드 매니저의 스피커 부족!!");
            }
        }
        else
            Debug.LogWarning("전역사운드가 잘못된 사운드를 요청하였음!!");

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

                    sources.Add(source); // 비활성화된 사운드 목록에 추가
                    activeSources.Remove(source); // 활성화된 사운드 목록에서 제거

                    obj = source.gameObject;
                    DefaultSourceData data = obj.GetComponent<DefaultSourceData>();
                    data.maxDistance = 15f;
                    obj.transform.SetParent(transform);
                    if (data.myCoroutine != null)
                        StopCoroutine(data.myCoroutine);
                    data.myCoroutine = null;

                    break;
                }
            }
        }
        else
            Debug.LogWarning(caller + " 이 잘못된 사운드를 정지하였음!!");
    }

    public void StopSound(AudioSource _audio)
    {
        if (_audio != null)
        {
            _audio.Stop(); // 사운드 정지

            sources.Add(_audio); // 비활성화된 사운드 목록에 추가
            activeSources.Remove(_audio); // 활성화된 사운드 목록에서 제거

            DefaultSourceData data = _audio.gameObject.GetComponent<DefaultSourceData>();
            data.maxDistance = 15f;
            _audio.transform.SetParent(transform);
            if (data.myCoroutine != null)
                StopCoroutine(data.myCoroutine);
            data.myCoroutine = null;
        }
    }

    IEnumerator StopTime(float time, GameObject obj)
    {
        yield return new WaitForSecondsRealtime(time * 1.5f);
        sources.Add(obj.GetComponent<AudioSource>());
        activeSources.Remove(obj.GetComponent<AudioSource>()); // 활성화된 사운드 목록에서 제거
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
        //// 화면 중앙 계산
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

        // 카메라 기준 계산
        foreach (AudioSource source in activeSources)
        {
            DefaultSourceData data = source.GetComponent<DefaultSourceData>();
            if(!isAllStop)
            {
                if(isLowResource)
                    data.RefreshVisibility();

                if (data.isVolCon)
                {
                    if (!isLowResource)  // 저사양모드 설정 아닐시
                    {
                        Vector2 pos = source.transform.position;

                        // 거리 기반으로 볼륨 조절
                        float distanceFromCenter = Vector2.Distance(pos, cam.transform.position);

                        float result = distanceFromCenter / data.maxDistance;

                        // 볼륨 조절
                        //source.volume = Mathf.Clamp01(1f - (distanceFromCenter / maxDistance));
                        switch (data.soundType)
                        {
                            case SoundType.Se:
                                source.volume = (1 - result) * seVol; break;
                            case SoundType.Bg:
                                source.volume = bgVol; break;
                            default:
                                source.volume = (1 - result) * seVol; break;
                        }
                        //Debug.Log($"[Sound] name: {source.clip?.name}, pos: {pos}, cam: {cam.transform.position}, dist: {distanceFromCenter}, maxDist: {data.maxDistance}, seVol: {seVol}, volume: {source.volume}, isAllStop: {isAllStop}");
                    }
                    else if(data.isVisible && isLowResource)
                    {
                        switch (data.soundType)
                        {
                            case SoundType.Se:
                                source.volume = seVol * data.volOverride;
                                break;
                            case SoundType.Bg:
                                source.volume = bgVol * data.volOverride;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // 화면안에 없으며 백사운드 아니면 무시
                        source.volume = 0f;
                        //continue;
                    }

                    //Debug.Log($"[Sound] name: {source.clip?.name}, cam: {cam.transform.position}, maxDist: {data.maxDistance}, seVol: {seVol}, volume: {source.volume}, isAllStop: {isAllStop}");
                }
                else
                {
                    switch (data.soundType)
                    {
                        case SoundType.Se:
                            source.volume = seVol * data.volOverride;
                            break;
                        case SoundType.Bg:
                            source.volume = bgVol * data.volOverride;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                source.volume = 0f;
            }
        }

        //// 화면 중앙 계산
        //Vector2 leftDown = cam.ScreenToWorldPoint(new Vector2(0, 0));
        //Vector2 rightUp = cam.ScreenToWorldPoint(new Vector2(cam.scaledPixelWidth, cam.scaledPixelHeight));

        //foreach (var source in activeSources)
        //{
        //    Vector2 pos = source.transform.position;

        //    // 오브젝트가 화면 경계 내에 있는지 확인
        //    bool isInside = (pos.x >= leftDown.x && pos.x <= rightUp.x) &&
        //                    (pos.y >= leftDown.y && pos.y <= rightUp.y);

        //    if (isInside)
        //    {
        //        source.GetComponent<DefaultSourceData>().volume = 1f; // 화면 안에 있을 때 볼륨 최대로 유지
        //    }
        //    else
        //    {
        //        source.GetComponent<DefaultSourceData>().volume = 0f;
        //    }
        //}
    }

    public void SetMute(bool _value, float _timer = 0f)
    {
        StartCoroutine(StopAllSound(_value, _timer));
    }

    public void ForceSetMute(bool _value)
    {
        isAllStop = _value;

        if (isAllStop)
        {
            foreach (AudioSource source in activeSources)
                source.volume = 0.0f;
        }
    }


    IEnumerator StopAllSound(bool _value, float _timer)
    {
        yield return new WaitForSecondsRealtime(_timer);
        isAllStop = _value;

        if (!isAllStop)
        {
            BgmPlayer.instance.StartBgmPlayer();
        }
    }

    public void SetBgVolume(Slider slider)
    {
        if(instance)
            bgVol = slider.value;
    }

    public void SetSeVolume(Slider slider)
    {
        if (instance)
            seVol = slider.value;
    }

    public void SetLowRes(bool val)
    {
        isLowResource = val;
    }
}

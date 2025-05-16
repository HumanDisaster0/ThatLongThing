using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    [SerializeField] int sourceSize = 10;
    [SerializeField] List<AudioSource> sources;

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

        LoadAllSound();
    }

    // Update is called once per frame
    void Update()
    {
        if(sources.Count != transform.childCount)
        {
            sources = GetComponentsInChildren<AudioSource>().ToList();
        }
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
                sources[0].clip = _clip;
                sources[0].gameObject.transform.position =
                    new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                sources[0].loop = false;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                audioSource = sources[0];
                sources[0].gameObject.transform.SetParent(caller.transform);
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
                    sources[0].clip = _clip;
                    sources[0].gameObject.transform.position =
                        new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                    sources[0].loop = false;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);
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
}

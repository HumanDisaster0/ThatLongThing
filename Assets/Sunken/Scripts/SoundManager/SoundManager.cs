using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] Dictionary<string, AudioClip> soundClips = new Dictionary<string, AudioClip>();

    [SerializeField] int sourceSize = 10;
    [SerializeField] List<AudioSource> sources;
    [SerializeField] List<AudioSource> activeSources;

    [Header("���� ����")]
    [Range(0f,1f)]
    public float seVol = 1f; // SE
    [Range(0f, 1f)]
    public float bgVol = 1f; // BGM

    Camera cam;
    bool isAllStop = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        while (sources.Count < sourceSize)
        {
            GameObject loaded = Resources.Load<GameObject>("Sound/Prefab/DefaultSource");
            GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(loaded);
            obj.transform.SetParent(transform);
            sources.Add(obj.GetComponent<AudioSource>());
        }
    }
#endif

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        cam = Camera.main;
        sources = GetComponentsInChildren<AudioSource>().ToList();

        LoadAllSound();

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckVolume();
    }

    private void OnLevelWasLoaded(int level)
    {
        cam = Camera.main;
        if (!cam)
            Debug.LogError("ī�޶� ���ٰ�? ��¥?");
    }

    /// <summary>
    /// �ߺ� ������� ���
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlayNewSound(string soundName, GameObject caller)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].gameObject.transform.position =
                    new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                sources[0].loop = false;
                sources[0].volume = 1f;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                audioSource = sources[0];

                sources[0].gameObject.transform.SetParent(caller.transform);

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlayNewSound(string soundName, GameObject caller, float maxDistance)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].gameObject.transform.position =
                    new Vector3(caller.transform.position.x, caller.transform.position.y, sources[0].transform.position.z);
                sources[0].loop = false;
                sources[0].volume = 1f;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                audioSource = sources[0];

                sources[0].gameObject.transform.SetParent(caller.transform);

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlayNewBackSound(string soundName)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            if (sources.Count != 0)
            {
                sources[0].clip = _clip;
                sources[0].gameObject.transform.position =
                    new Vector3(cam.transform.position.x, cam.transform.position.y, sources[0].transform.position.z);
                sources[0].loop = false;
                sources[0].volume = 1f;
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = false;
                audioSource = sources[0];

                sources[0].gameObject.transform.SetParent(cam.transform);

                activeSources.Add(sources[0]);
                sources.Remove(sources[0]);

            }
            else
                Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
        }
        else
            Debug.LogError(cam + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    /// <summary>
    /// �ߺ����� ���
    /// </summary>
    /// <param name="soundName"></param>
    /// <param name="caller"></param>
    public AudioSource PlaySound(string soundName, GameObject caller)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlaySound(string soundName, GameObject caller, float maxDistance)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlayBackSound(string soundName)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 ������� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = cam.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                        new Vector3(cam.transform.position.x, cam.transform.position.y, sources[0].transform.position.z);
                    sources[0].loop = false;
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                        StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = false;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(cam.transform);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(cam + " �� �߸��� ���带 ��û�Ͽ���!! : " + soundName);

        return audioSource;
    }

    public AudioSource PlayLoopSound(string soundName, GameObject caller)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 �ִ� ��� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlayLoopSound(string soundName, GameObject caller, float maxDistance)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 �ִ� ��� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].gameObject.GetComponent<DefaultSourceData>().isVolCon = true;
                    sources[0].gameObject.GetComponent<DefaultSourceData>().maxDistance = maxDistance;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(caller.transform);
                    
                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public AudioSource PlayLoopBackSound(string soundName)
    {
        AudioSource audioSource = null;

        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 �ִ� ��� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = cam.GetComponentsInChildren<AudioSource>();
            bool isExist = false;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
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
                        new Vector3(cam.transform.position.x, cam.transform.position.y, sources[0].transform.position.z);
                    sources[0].loop = true;
                    sources[0].volume = 1f;
                    sources[0].Play();
                    sources[0].GetComponent<DefaultSourceData>().isVolCon = false;
                    audioSource = sources[0];
                    sources[0].gameObject.transform.SetParent(cam.transform);

                    activeSources.Add(sources[0]);
                    sources.Remove(sources[0]);
                }
                else
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(cam + " �� �߸��� ���带 ��û�Ͽ���!!");

        return audioSource;
    }

    public void StopSound(string soundName, GameObject caller)
    {
        if (soundClips.TryGetValue(soundName, out AudioClip _clip)) // ���尡 �ִ� ��� ����
        {
            // �ڽ� ������Ʈ���� ��� AudioSource ��������
            AudioSource[] objSources = caller.GetComponentsInChildren<AudioSource>();
            GameObject obj = null;

            foreach (AudioSource source in objSources)
            {
                if (source.clip == _clip) // ���� ������� soundName�� ���� ���
                {
                    source.Stop(); // ���� ����

                    sources.Add(source); // ��Ȱ��ȭ�� ���� ��Ͽ� �߰�
                    activeSources.Remove(source); // Ȱ��ȭ�� ���� ��Ͽ��� ����

                    obj = source.gameObject;
                    DefaultSourceData data = obj.GetComponent<DefaultSourceData>();
                    data.maxDistance = 20f;
                    obj.transform.SetParent(transform);
                    if (data.myCoroutine != null)
                        StopCoroutine(data.myCoroutine);
                    data.myCoroutine = null;

                    break;
                }
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 �����Ͽ���!!");
    }

    public void StopSound(AudioSource _audio)
    {
        if (_audio != null)
        {
            _audio.Stop(); // ���� ����

            sources.Add(_audio); // ��Ȱ��ȭ�� ���� ��Ͽ� �߰�
            activeSources.Remove(_audio); // Ȱ��ȭ�� ���� ��Ͽ��� ����

            DefaultSourceData data = _audio.gameObject.GetComponent<DefaultSourceData>();
            data.maxDistance = 20f;
            _audio.transform.SetParent(transform);
            if (data.myCoroutine != null)
                StopCoroutine(data.myCoroutine);
            data.myCoroutine = null;
        }
    }

    IEnumerator StopTime(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
        sources.Add(obj.GetComponent<AudioSource>());
        activeSources.Remove(obj.GetComponent<AudioSource>()); // Ȱ��ȭ�� ���� ��Ͽ��� ����
        obj.transform.SetParent(transform);
    }

    private void LoadAllSound()
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Sound"); // "Resources/Sound" ���� ���� ��� Ŭ���� �ε�

        foreach (AudioClip clip in clips)
        {
            if (!soundClips.ContainsKey(clip.name)) // �ߺ� ����
            {
                soundClips.Add(clip.name, clip);
            }
        }

        Debug.Log("�� " + soundClips.Count + "���� ���� �ε� �Ϸ�!");
    }

    private void CheckVolume()
    {
        //// ȭ�� �߾� ���
        //Vector2 leftDown = cam.ScreenToWorldPoint(new Vector2(0, 0));
        //Vector2 rightUp = cam.ScreenToWorldPoint(new Vector2(cam.scaledPixelWidth, cam.scaledPixelHeight));

        //foreach (var source in allSources)
        //{
        //    Vector2 pos = source.transform.position;

        //    // ������Ʈ�� ȭ�� ��� ���� �ִ��� Ȯ��
        //    bool isInside = (pos.x >= leftDown.x && pos.x <= rightUp.x) &&
        //                    (pos.y >= leftDown.y && pos.y <= rightUp.y);

        //    if (isInside)
        //    {
        //        source.volume = 1f; // ȭ�� �ȿ� ���� �� ���� �ִ�� ����
        //    }
        //    else
        //    {
        //        // ȭ�� ��迡�� �󸶳� �־������� ���
        //        float distance = Mathf.Min(Vector2.Distance(pos, leftDown), Vector2.Distance(pos, rightUp));

        //        // �Ÿ��� �־������� ���� ���� (�ּҰ� 0 ~ �ִ밪 1)
        //        source.volume = Mathf.Clamp01(1f - (distance / volumeGradation)); // `10f`�� ���� ������ ���� �Ÿ�
        //    }
        //}

        // ī�޶� ���� ���
        foreach (AudioSource source in activeSources)
        {
            DefaultSourceData data = source.GetComponent<DefaultSourceData>();
            if(!isAllStop)
            {
                if (data.isVolCon)
                {
                    // ȭ��ȿ� ������ ����
                    if (!data.isVisible)
                        continue;
                    else
                    {
                        Vector2 pos = source.transform.position;

                        // �Ÿ� ������� ���� ����
                        float distanceFromCenter = Vector2.Distance(pos, cam.transform.position);

                        float result = distanceFromCenter / data.maxDistance;

                        // ���� ����
                        //source.volume = Mathf.Clamp01(1f - (distanceFromCenter / maxDistance));
                        source.volume = (1 - result) * seVol;
                    }
                }
            }
            else
            {
                source.volume = 0f;
            }
        }

        //// ȭ�� �߾� ���
        //Vector2 leftDown = cam.ScreenToWorldPoint(new Vector2(0, 0));
        //Vector2 rightUp = cam.ScreenToWorldPoint(new Vector2(cam.scaledPixelWidth, cam.scaledPixelHeight));

        //foreach (var source in activeSources)
        //{
        //    Vector2 pos = source.transform.position;

        //    // ������Ʈ�� ȭ�� ��� ���� �ִ��� Ȯ��
        //    bool isInside = (pos.x >= leftDown.x && pos.x <= rightUp.x) &&
        //                    (pos.y >= leftDown.y && pos.y <= rightUp.y);

        //    if (isInside)
        //    {
        //        source.GetComponent<DefaultSourceData>().volume = 1f; // ȭ�� �ȿ� ���� �� ���� �ִ�� ����
        //    }
        //    else
        //    {
        //        source.GetComponent<DefaultSourceData>().volume = 0f;
        //    }
        //}
    }

    public void SetMute(bool _value)
    {
        isAllStop = _value;

        if(!isAllStop)
        {
            BgmPlayer.instance.StartBgmPlayer();
        }
    }
}

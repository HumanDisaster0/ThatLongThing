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
                sources[0].Play();
                sources[0].gameObject.GetComponent<DefaultSourceData>().myCoroutine =
                    StartCoroutine(StopTime(_clip.length, sources[0].gameObject));
                audioSource = sources[0];
                sources[0].gameObject.transform.SetParent(caller.transform);
            }
            else
                Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

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
                    Debug.LogWarning("���� �Ŵ����� ����Ŀ ����!!");
            }
        }
        else
            Debug.LogError(caller + " �� �߸��� ���带 ��û�Ͽ���!!");

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
            Debug.LogError(caller + " �� �߸��� ���带 �����Ͽ���!!");
    }

    IEnumerator StopTime(float time, GameObject obj)
    {
        yield return new WaitForSeconds(time);
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
}

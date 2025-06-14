using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionCheckCanvas : MonoBehaviour
{
    static public MissionCheckCanvas instance = null;

    private bool isActive = false;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        if(transform.GetChild(0).gameObject.activeSelf)
            transform.GetChild(0).gameObject.SetActive(false);
    }

    private void Update()
    {
        if(isActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.M))
            {
                CloseMission();
            }
        }
    }

    public void ShowMission(Sprite _sprite)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        isActive = true;
        Image missionImage = transform.Find("Panel/MissionImage").GetComponent<Image>();
        if (_sprite != null && missionImage != null)
            missionImage.sprite = _sprite;

        // ���� ��Ȱ��ȭ
        if(GuildRoomManager.Instance != null)
        {
            GuildRoomManager.Instance.isPauseAble = false;
        }

        // ���� ���� ���߱�
        //SoundManager.instance?.SetMute(true);

        // ���� �Ͻ�����
        Time.timeScale = 0f;
    }

    public void CloseMission()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        isActive = false;

        // ���� Ȱ��ȭ
        // ���� ��Ȱ��ȭ
        if (GuildRoomManager.Instance != null)
        {
            GuildRoomManager.Instance.isPauseAble = true;
        }

        // ���� ���� �簳
        //SoundManager.instance?.SetMute(false);

        // ���� �Ͻ����� ����
        Time.timeScale = 1f;
    }
}

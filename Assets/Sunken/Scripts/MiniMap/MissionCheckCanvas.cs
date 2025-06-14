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

        // 퍼즈 비활성화
        if(GuildRoomManager.Instance != null)
        {
            GuildRoomManager.Instance.isPauseAble = false;
        }

        // 게임 사운드 멈추기
        //SoundManager.instance?.SetMute(true);

        // 게임 일시정지
        Time.timeScale = 0f;
    }

    public void CloseMission()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        isActive = false;

        // 퍼즈 활성화
        // 퍼즈 비활성화
        if (GuildRoomManager.Instance != null)
        {
            GuildRoomManager.Instance.isPauseAble = true;
        }

        // 게임 사운드 재개
        //SoundManager.instance?.SetMute(false);

        // 게임 일시정지 해제
        Time.timeScale = 1f;
    }
}

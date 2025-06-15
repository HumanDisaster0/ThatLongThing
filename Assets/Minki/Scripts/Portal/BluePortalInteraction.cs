using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BluePortalInteraction : MonoBehaviour
{
    GameObject m_playerPin;
    MapMatchCheckPinCreator m_pinCreator;
    MapOnOffControl m_mapOnOffControl;
    MapPinMatchChecker m_pinMatchChecker;
    MinimapDestroyer m_mapDestroyer;

    public void Start()
    {
        m_playerPin = GameObject.FindFirstObjectByType<MinimapPlayerPos>(FindObjectsInactive.Include).gameObject;
        m_pinCreator = GameObject.FindFirstObjectByType<MapMatchCheckPinCreator>(FindObjectsInactive.Include);
        m_mapOnOffControl = GameObject.FindFirstObjectByType<MapOnOffControl>(FindObjectsInactive.Include);
        m_pinMatchChecker = GameObject.FindFirstObjectByType<MapPinMatchChecker>(FindObjectsInactive.Include);
        m_mapDestroyer = GameObject.FindFirstObjectByType<MinimapDestroyer>(FindObjectsInactive.Include);
    }

    public void EnterPortal()
    {
        if(m_mapDestroyer.IsDestroied)
        {
            StageManager.instance.EndStage();
            return;
        }

        //미니맵 켜기
        m_mapOnOffControl.ShowMinimap();
        m_pinCreator.UpdateSibling();

        //재시작 버튼 끄기
        GameObject.Find("재시작").SetActive(false);

        //플레이어 핀 가리기
        m_playerPin.gameObject.SetActive(false);

        //매치체크 핀 생성
        m_pinCreator.CreatePins(m_pinMatchChecker.CreateMatchData());

        //미니맵 핀 상호작용 중지(핀클릭 금지!)
        MapPinSetter.IsPinSetterActive = false;

        //미니맵 끌 씨 다음 코루틴 실행 등록
        m_mapOnOffControl.OnMiniMapHide += StartNextEvent;
    }

    private void OnDestroy()
    {
        if(m_mapOnOffControl != null)
        {
            m_mapOnOffControl.OnMiniMapHide -= StartNextEvent;
        }
    }

    public void StartNextEvent()
    {
        StartCoroutine(GoGuildRoom());
    }

    IEnumerator GoGuildRoom()
    {
        //미니맵 클릭 금지
        m_mapOnOffControl.activeControl = false;

        //미니맵 핀 상호작용 풀기
        MapPinSetter.IsPinSetterActive = true;

        yield return null;

        var timer = 0.0f;

        while(timer < 1.25f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        StageManager.instance.EndStage();
    }
}
